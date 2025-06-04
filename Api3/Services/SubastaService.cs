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

            if (pokemon == null || monto <= pokemon.PujaActual)
            {
                return false;
            }

            pokemon.PujaActual = monto;
            pokemon.Email = usuario;

            if (pokemon.HistorialPujas == null)
            {
                pokemon.HistorialPujas = new List<Puja2>();
            }

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
        public async Task<bool> FinalizarSubastaAsync(int pokemonId)
        {
            var productoPokemon = await _context.ProductoPokemon
                .Include(p => p.HistorialPujas)
                .FirstOrDefaultAsync(p => p.Id == pokemonId);

            if (productoPokemon == null || !productoPokemon.EnVenta || DateTime.Now < productoPokemon.TiempoExpiracion)
            {
                return false; 
            }

           
            var ultimaPuja = productoPokemon.HistorialPujas.OrderByDescending(p => p.FechaPuja).FirstOrDefault();
            if (ultimaPuja == null)
            {
                return false; 
            }

          
            productoPokemon.EnVenta = false;
            _context.ProductoPokemon.Attach(productoPokemon);
            _context.Entry(productoPokemon).Property(p => p.EnVenta).IsModified = true; 

       
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Subasta finalizada. Estado de venta actualizado para el Pokémon {productoPokemon.Nombre}");

        
            await _hubContext.Clients.All.SendAsync("SubastaFinalizada", pokemonId, ultimaPuja.UsuarioEmail);

            return true;
        }











    }
}
