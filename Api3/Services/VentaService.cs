using api3.Models;
using Microsoft.AspNetCore.Mvc;
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
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("❌ Error: El email del usuario no puede estar vacío.");

            var pokemons = _context.ProductoPokemon
                .Include(p => p.Stats)
                .Where(p => p.Email == email && p.EnVenta == true) 
                .ToList();

            return pokemons;
        }
        public void PonerPokemonEnSubasta(string emailUsuario, int pokemonId, decimal precioInicial, int duracionMinutos)
        {
            var usuario = _context.UsuariosPokemonApi.FirstOrDefault(u => u.Email == emailUsuario);
            var pokemon = _context.ProductoPokemon.Find(pokemonId);

            if (usuario == null || pokemon == null)
                throw new ArgumentException("❌ Error: No se encontró el usuario o el Pokémon.");

            // Validar que el dueño sea quien pone en subasta
            if (pokemon.Email != emailUsuario)
                throw new InvalidOperationException("❌ No puedes subastar un Pokémon que no es tuyo.");

            // Configurar los valores de la subasta
            pokemon.PrecioInicial = precioInicial;
            pokemon.PujaActual = precioInicial; // Comienza con el precio establecido por el dueño
            pokemon.TiempoExpiracion = DateTime.Now.AddMinutes(duracionMinutos);
            pokemon.EnVenta = true; // Se activa la venta

            _context.SaveChanges();
        }
      


















    }
}
