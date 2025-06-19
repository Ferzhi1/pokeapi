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

                    if (nuevoTiempo <= 0)
                    {
                        await _hubContext.Clients.All.SendAsync("ActualizarTiempoSubasta", pokemonId, emailVendedor, "⏳ Finalizando...");
                        await Task.Delay(5000);

                        if (SubastasActivas.TryRemove(pokemonId, out var finalSubasta))
                        {
                            finalSubasta.Item1?.Dispose();
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
                SubastasActivas[pokemonId] = (timer, tiempoRestante);
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
            var pokemon = await _context.ProductoPokemon
                .Include(p => p.HistorialPujas)
                .FirstOrDefaultAsync(p => p.Id == oferta.PokemonId);

            if (pokemon == null)
            {
                return new ResultadoSubasta { SinPujas = true, NombrePokemon = "Desconocido" };
            }

            var pujaGanadora = await _context.Puja
                .Where(p =>
                    p.PokemonId == oferta.PokemonId &&
                    p.FechaPuja >= pokemon.fechaInicioSubasta &&
                    p.FechaPuja <= pokemon.TiempoExpiracion)
                .OrderByDescending(p => p.CantidadMonedas)
                .FirstOrDefaultAsync();

            if (pujaGanadora == null)
            {
                pokemon.EnVenta = true;
                pokemon.Precio = 0;

                _context.ProductoPokemon.Update(pokemon);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("FinalizarSubasta", oferta.PokemonId, pokemon.Nombre, 0, "Sin ganador", 0m);
                await _hubContext.Clients.User(pokemon.Email).SendAsync("PokemonDevuelto", oferta.PokemonId, pokemon.Nombre);

                return new ResultadoSubasta { SinPujas = true, NombrePokemon = pokemon.Nombre };
            }

            var comprador = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == pujaGanadora.UsuarioEmail);
            var vendedor = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == pokemon.UltimoDueno);

            bool tieneDescuento = await _context.SolicitudAmistad.AnyAsync(s =>
                ((s.RemitenteEmail == pujaGanadora.UsuarioEmail && s.ReceptorEmail == pokemon.UltimoDueno) ||
                 (s.RemitenteEmail == pokemon.UltimoDueno && s.ReceptorEmail == pujaGanadora.UsuarioEmail)) &&
                s.Estado == EstadoSolicitud.Aceptada);

            decimal precioFinal = tieneDescuento
                ? pujaGanadora.CantidadMonedas * 0.7m
                : pujaGanadora.CantidadMonedas;

            pokemon.Email = pujaGanadora.UsuarioEmail;
            pokemon.EnVenta = true;
            pokemon.Precio = precioFinal;

            _context.ProductoPokemon.Update(pokemon);
            await _context.SaveChangesAsync();

            if (comprador != null && vendedor != null)
            {
                Console.WriteLine($"Puja: {pujaGanadora.CantidadMonedas}, Descuento aplicado: {tieneDescuento}, Monto final: {precioFinal}");
                Console.WriteLine($"Monedero comprador antes: {comprador.Monedero}");

                comprador.Monedero -= precioFinal;
                vendedor.Monedero += precioFinal;

                _context.Entry(comprador).State = EntityState.Modified;
                _context.Entry(vendedor).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                Console.WriteLine($"Monedero comprador después: {comprador.Monedero}");
            }

            if (SubastaHub.UsuariosSubasta.TryGetValue(comprador.Email, out var compradorConnectionId))
            {
                await _hubContext.Clients.Client(compradorConnectionId).SendAsync("ActualizarMonedero", comprador.Monedero);
            }

            if (vendedor != null && SubastaHub.UsuariosSubasta.TryGetValue(vendedor.Email, out var vendedorConnectionId))
            {
                await _hubContext.Clients.Client(vendedorConnectionId).SendAsync("ActualizarMonedero", vendedor.Monedero);
            }


            return new ResultadoSubasta
            {
                SinPujas = false,
                NombrePokemon = pokemon.Nombre,
                Ganador = pujaGanadora.UsuarioEmail
            };
        }
    }
}











   
