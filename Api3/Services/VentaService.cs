using api3.Models;
using Microsoft.EntityFrameworkCore;

namespace api3.Services
{
    public class VentaService
    {
        private readonly ApplicationDbContext _context;

        public VentaService(ApplicationDbContext context)
        {
            _context = context;
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

        public void PonerPokemonEnSubasta(string emailUsuario, int pokemonId, decimal precioInicial, int duracionMinutos)
        {
            var usuario = _context.UsuariosPokemonApi.FirstOrDefault(u => u.Email == emailUsuario);
            var pokemon = _context.ProductoPokemon.Find(pokemonId);

            if (usuario == null || pokemon == null) return;
            if (pokemon.Email != emailUsuario) return;

            pokemon.PrecioInicial = precioInicial;
            pokemon.PujaActual = precioInicial;
            pokemon.TiempoExpiracion = DateTime.Now.AddMinutes(duracionMinutos);
            pokemon.EnVenta = true;

            _context.SaveChanges();
        }
    }
}
