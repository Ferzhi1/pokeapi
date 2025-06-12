using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using api3.Hubs;
using Api3.Models;
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

            pokemon.PujaActual = monto;
            pokemon.Email = usuario;

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

                        await FinalizarSubastaAsync(pokemonId);
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

        public async Task FinalizarSubastaAsync(int pokemonId)
        {
            if (SubastasActivas.TryRemove(pokemonId, out var subasta))
            {
                var pokemon = await _context.ProductoPokemon
                    .Include(p => p.HistorialPujas)
                    .FirstOrDefaultAsync(p => p.Id == pokemonId);

                if (pokemon == null) return;

                var pujaGanadora = pokemon.HistorialPujas?.OrderByDescending(p => p.CantidadMonedas).FirstOrDefault();

                if (pujaGanadora != null)
                {
                    pokemon.Email = pujaGanadora.UsuarioEmail;
                }

                pokemon.EnVenta = false;

                _context.ProductoPokemon.Update(pokemon);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("FinalizarSubasta", pokemon.Nombre);
            }
        }


    }
}
