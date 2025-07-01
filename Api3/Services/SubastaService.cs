using System.Collections.Concurrent;
using api3.Hubs;
using api3.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api3.Services
{
    public class SubastaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SubastaHub> _hubContext;
        private static ConcurrentDictionary<int, (Timer, int)> SubastasActivas = new ConcurrentDictionary<int, (Timer, int)>();
        private static ConcurrentDictionary<int, object> Locks = new();
        public static ConcurrentDictionary<string, string> UsuariosSubasta = new();
        public SubastaService(ApplicationDbContext context, IHubContext<SubastaHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }


        public async Task<bool> RegistrarOfertaAsync(int pokemonId, string usuario, decimal monto)
        {
            var pokemon = await _context.ProductoPokemon
                .Include(p => p.HistorialPujas)
                .FirstOrDefaultAsync(p => p.Id == pokemonId);

            if (pokemon == null || monto <= pokemon.PujaActual) return false;
            var usuarioPokemon = await _context.UsuariosPokemonApi
                .FirstOrDefaultAsync(u => u.Email == usuario);

            if (usuarioPokemon == null || usuarioPokemon.Monedero < monto)
                return false;

            pokemon.PujaActual = monto;
            pokemon.Email = usuario;
            pokemon.Precio = monto;

            pokemon.HistorialPujas ??= new List<Puja2>();

            pokemon.HistorialPujas.Add(new Puja2
            {
                PokemonId = pokemonId,
                UsuarioEmail = usuario,
                CantidadMonedas = monto,
                FechaPuja = DateTime.Now
            });

            _context.ProductoPokemon.Update(pokemon);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ActualizarOferta", pokemonId, usuario, monto);

            return true;
        }


        public void IniciarTemporizador(int pokemonId, string emailVendedor, int duracionMinutos)
        {
            int tiempoRestante = duracionMinutos * 60;

            Timer timer = null;

            timer = new Timer(async _ =>
            {
                if (SubastasActivas.TryGetValue(pokemonId, out var subasta))
                {
                    int nuevoTiempo = subasta.Item2 - 1;

                    Console.WriteLine($"[TIMER] Pokémon ID: {pokemonId} - Tiempo restante: {nuevoTiempo}s");

                    if (nuevoTiempo <= 0)
                    {
                        Console.WriteLine($"[TIMER] Pokémon ID: {pokemonId} - Tiempo agotado. Finalizando...");

                        await _hubContext.Clients.All.SendAsync("ActualizarTiempoSubasta", pokemonId, emailVendedor, "⏳ Finalizando...");
                        await Task.Delay(5000);

                        if (SubastasActivas.TryRemove(pokemonId, out var finalSubasta))
                        {
                            finalSubasta.Item1?.Dispose();
                            Console.WriteLine($"[TIMER] Pokémon ID: {pokemonId} - Temporizador eliminado.");
                        }
                    }
                    else
                    {
                        SubastasActivas[pokemonId] = (timer, nuevoTiempo);
                        await _hubContext.Clients.All.SendAsync("ActualizarTiempoSubasta", pokemonId, emailVendedor, nuevoTiempo);
                    }
                }
            }, null, 1000, 1000);

            if (SubastasActivas.ContainsKey(pokemonId))
            {
                Console.WriteLine($"[TIMER] Pokémon ID: {pokemonId} - Temporizador sobrescrito con {tiempoRestante}s.");
                SubastasActivas[pokemonId] = (timer, tiempoRestante);
            }
            else
            {
                Console.WriteLine($"[TIMER] Pokémon ID: {pokemonId} - Nuevo temporizador creado con {tiempoRestante}s.");
            }
        }

        public async Task<ResultadoSubasta> FinalizarSubastaAsync(OfertaDto oferta)
        {
            var lockObj = Locks.GetOrAdd(oferta.PokemonId, new object());

            lock (lockObj)
            {
                return FinalizarInternamenteAsync(oferta).GetAwaiter().GetResult();
            }
        }


        private async Task<ResultadoSubasta> FinalizarInternamenteAsync(OfertaDto oferta)
        {
            var txId = Guid.NewGuid();
            Console.WriteLine($"[{txId}] 🟡 Iniciando finalización de subasta para Pokémon ID: {oferta.PokemonId}");

            var pokemon = await _context.ProductoPokemon
                .Include(p => p.HistorialPujas)
                .FirstOrDefaultAsync(p => p.Id == oferta.PokemonId);

            if (pokemon == null)
            {
                Console.WriteLine($"[{txId}] ⚠️ Pokémon no encontrado.");
                return new ResultadoSubasta { SinPujas = true, NombrePokemon = "Desconocido" };
            }

            if (!pokemon.EnVenta)
            {
                Console.WriteLine($"[{txId}] ⛔ Subasta ya finalizada previamente para Pokémon ID: {oferta.PokemonId}. Abortando ejecución.");
                return new ResultadoSubasta { SinPujas = false, NombrePokemon = pokemon.Nombre, Ganador = pokemon.Email };
            }

            var pujaGanadora = await _context.Puja
                .Where(p => p.PokemonId == oferta.PokemonId &&
                            p.FechaPuja >= pokemon.fechaInicioSubasta &&
                            p.FechaPuja <= pokemon.TiempoExpiracion)
                .OrderByDescending(p => p.CantidadMonedas)
                .FirstOrDefaultAsync();

            if (pujaGanadora == null)
            {
                Console.WriteLine($"[{txId}] ❌ No hubo pujas. Pokémon se queda con el vendedor.");
                pokemon.EnVenta = false;
                pokemon.Precio = 0;

                _context.Update(pokemon);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("FinalizarSubasta", oferta.PokemonId, pokemon.Nombre, 0, "Sin ganador", 0m);
                await _hubContext.Clients.User(pokemon.Email).SendAsync("PokemonDevuelto", oferta.PokemonId, pokemon.Nombre);

                return new ResultadoSubasta { SinPujas = true, NombrePokemon = pokemon.Nombre };
            }

            var comprador = await _context.UsuariosPokemonApi.AsNoTracking().FirstOrDefaultAsync(u => u.Email == pujaGanadora.UsuarioEmail);
            var vendedor = await _context.UsuariosPokemonApi.AsNoTracking().FirstOrDefaultAsync(u => u.Email == pokemon.UltimoDueno);


            bool tieneDescuento = await _context.SolicitudAmistad.AnyAsync(s =>
                ((s.RemitenteEmail == comprador.Email && s.ReceptorEmail == vendedor.Email) ||
                 (s.RemitenteEmail == vendedor.Email && s.ReceptorEmail == comprador.Email)) &&
                s.Estado == EstadoSolicitud.Aceptada);

            decimal precioFinal = tieneDescuento ? pujaGanadora.CantidadMonedas * 0.7m : pujaGanadora.CantidadMonedas;

            Console.WriteLine($"[{txId}] ✅ Puja ganadora por {pujaGanadora.CantidadMonedas}, descuento: {tieneDescuento}, total: {precioFinal}");

            pokemon.Email = comprador.Email;
            pokemon.EnVenta = false;
            pokemon.Precio = precioFinal;


            comprador.Monedero -= precioFinal;
            vendedor.Monedero += precioFinal;

            _context.UpdateRange(pokemon, comprador, vendedor);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[{txId}] 💰 Monedero comprador: {comprador.Monedero} | vendedor: {vendedor.Monedero}");

            string compradorConn;
            if (SubastaHub.UsuariosSubasta.TryGetValue(comprador.Email, out compradorConn))
            {
                Console.WriteLine($"📢 Enviando actualización de monedero al comprador: {comprador.Email} - ConnID: {compradorConn}");
                await _hubContext.Clients.Client(compradorConn).SendAsync("ActualizarMonedero", comprador.Monedero);
            }


            string vendedorConn;
            if (SubastaHub.UsuariosSubasta.TryGetValue(vendedor.Email, out vendedorConn))
            {
                Console.WriteLine($"📢 Enviando actualización de monedero al vendedor: {vendedor.Email} - ConnID: {vendedorConn}");
                await _hubContext.Clients.Client(vendedorConn).SendAsync("ActualizarMonedero", vendedor.Monedero);
            }




            Console.WriteLine($"[{txId}] ✅ Finalización completa para Pokémon {pokemon.Nombre}");

            return new ResultadoSubasta
            {
                SinPujas = false,
                NombrePokemon = pokemon.Nombre,
                Ganador = comprador.Email
            };
        }

        public async Task<List<ProductoPokemon>> ObtenerPokemonesGanadosPorUsuario(string emailUsuario)
        {
            
            var idsYaGuardados = await _context.ColeccionPokemon
                .Where(c => c.EmailUsuario == emailUsuario)
                .Select(c => c.PokemonIdOriginal)
                .ToListAsync();

         
            return await _context.ProductoPokemon
                .Where(p =>
                    !p.EnVenta &&
                    p.Email == emailUsuario &&
                    p.HistorialPujas.Any() &&
                    !idsYaGuardados.Contains(p.PokemonIdOriginal))
                .OrderByDescending(p => p.TiempoExpiracion)
                .ToListAsync();
        }

    }

}














