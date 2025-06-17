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
                if (SubastasActivas.TryGetValue(pokemonId, out var subasta) && subasta.Item2 > 0)
                {
                    tiempoRestante--;
                    SubastasActivas[pokemonId] = (timer, tiempoRestante);

                    if (tiempoRestante == 0)
                    {
                        await _hubContext.Clients.All.SendAsync("ActualizarTiempoSubasta", pokemonId, emailVendedor, "⏳ Finalizando...");

                        await Task.Delay(5000);

                        timer.Dispose();


                    }
                    else
                    {
                        await _hubContext.Clients.All.SendAsync("ActualizarTiempoSubasta", pokemonId, emailVendedor, tiempoRestante);
                    }
                }
            }, null, 0, 1000);

            SubastasActivas.TryAdd(pokemonId, (timer, tiempoRestante));
        }
        public async Task<ResultadoSubasta> FinalizarSubastaAsync(OfertaDto oferta)
        {
            if (!SubastasActivas.TryRemove(oferta.PokemonId, out _))
                return new ResultadoSubasta { SinPujas = true, NombrePokemon = "Desconocido" };

            var pokemon = await _context.ProductoPokemon
                .Include(p => p.HistorialPujas)
                .FirstOrDefaultAsync(p => p.Id == oferta.PokemonId);

            if (pokemon == null)
                return new ResultadoSubasta { SinPujas = true, NombrePokemon = "Desconocido" };

            var pujaGanadora = await _context.Puja
                .Where(p => p.PokemonId == oferta.PokemonId)
                .OrderByDescending(p => p.CantidadMonedas)
                .FirstOrDefaultAsync();

            if (pujaGanadora == null)
            {
                pokemon.EnVenta = false;
                pokemon.Precio = 0;

                _context.ProductoPokemon.Update(pokemon);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("FinalizarSubasta", oferta.PokemonId, pokemon.Nombre, 0, "Sin ganador", 0m);
                await _hubContext.Clients.User(pokemon.Email).SendAsync("PokemonDevuelto", oferta.PokemonId, pokemon.Nombre);
                await _hubContext.Clients.All.SendAsync("EliminarCarta", oferta.PokemonId);

                return new ResultadoSubasta { SinPujas = true, NombrePokemon = pokemon.Nombre };
            }

            string emailVendedorOriginal = pokemon.Email;

            bool tieneDescuento = await _context.SolicitudAmistad.AnyAsync(s =>
                ((s.RemitenteEmail == pujaGanadora.UsuarioEmail && s.ReceptorEmail == emailVendedorOriginal) ||
                 (s.ReceptorEmail == pujaGanadora.UsuarioEmail && s.RemitenteEmail == emailVendedorOriginal)) &&
                 s.Estado == EstadoSolicitud.Aceptada);

            decimal precioFinal = tieneDescuento ? pujaGanadora.CantidadMonedas * 0.7m : pujaGanadora.CantidadMonedas;

            pokemon.Email = pujaGanadora.UsuarioEmail;
            pokemon.EnVenta = false;
            pokemon.Precio = precioFinal;

            _context.ProductoPokemon.Update(pokemon);
            await _context.SaveChangesAsync();

            var comprador = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == pujaGanadora.UsuarioEmail);
            var vendedor = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == emailVendedorOriginal);

            if (comprador != null && vendedor != null)
            {
                comprador.Monedero -= precioFinal;
                vendedor.Monedero += precioFinal;

                _context.UsuariosPokemonApi.UpdateRange(comprador, vendedor);
                await _context.SaveChangesAsync();
            }

            await _hubContext.Clients.All.SendAsync("FinalizarSubasta", oferta.PokemonId, pokemon.Nombre, pujaGanadora.Id, pujaGanadora.UsuarioEmail, precioFinal);
            await _hubContext.Clients.User(pujaGanadora.UsuarioEmail).SendAsync("ActualizarMonedero", comprador?.Monedero ?? 0);
            await _hubContext.Clients.User(vendedor?.Email).SendAsync("ActualizarMonedero", vendedor?.Monedero ?? 0);

            return new ResultadoSubasta
            {
                SinPujas = false,
                NombrePokemon = pokemon.Nombre,
                Ganador = pujaGanadora.UsuarioEmail
            };
        }


    }
}