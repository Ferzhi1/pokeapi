using api3.Hubs;
using api3.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api3.Services
{
    public class VentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SubastaHub> _hubContext;

        public VentaService(ApplicationDbContext context, IHubContext<SubastaHub> hubContext)
        {
            _context = context;
            _hubContext= hubContext;
        }

        public void AgregarPokemonAVenta(string email, ProductoPokemon pokemon)
        {
            pokemon.Descripcion ??= "Sin descripción";
            _context.ProductoPokemon.Add(pokemon);
            _context.SaveChanges();
        }

        public List<ProductoPokemon> ObtenerVentaPokemon(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new List<ProductoPokemon>();

            return _context.ProductoPokemon
                .Include(p => p.Stats)
                .Where(p => p.Email == email && p.EnVenta)
                .ToList();
        }
        public async Task<bool> IniciarSubastaAsync(int pokemonId, decimal precioInicial, int duracionMinutos, string usuarioEmail)
        {
            var pokemon = await _context.ProductoPokemon.FirstOrDefaultAsync(p => p.Id == pokemonId && p.Email == usuarioEmail);

            if (pokemon == null)
            {
                return false;
            }

            pokemon.PrecioInicial = precioInicial;
            pokemon.PujaActual = precioInicial;
            pokemon.TiempoExpiracion = DateTime.Now.AddMinutes(duracionMinutos);
            pokemon.EnVenta = true;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("NuevaSubasta", pokemonId, pokemon.Nombre, precioInicial, duracionMinutos, pokemon.ImagenUrl);

            return true;
        }
    }
}
