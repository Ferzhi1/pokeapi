using api3.Models;
using Api3.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace api3.Services
{
    public class PokemonStorageService
    {
        private readonly ApplicationDbContext _context;

        public PokemonStorageService(ApplicationDbContext context) => _context = context;

        public UsuariosPokemonApi ObtenerUsuarioPokemon(string email) =>
            _context.UsuariosPokemonApi.FirstOrDefault(u => u.Email == email);

        public string ObtenerEmailUsuario(string email)
        {
            var usuarioPokemon = _context.UsuariosPokemonApi
                .FirstOrDefault(u => u.Email.Trim().ToLower() == email.Trim().ToLower());

            return usuarioPokemon?.Email ?? throw new InvalidOperationException("El usuario no existe en la base de datos.");
        }

        public void GuardarPedidoUsuario(string email, string mazo, List<PedidoPokemon> pokemons)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(mazo) || pokemons is not { Count: > 0 })
                throw new ArgumentException("El email, el mazo y la lista de Pokémon no pueden estar vacíos.");

            pokemons.ForEach(p => p.UsuarioEmail = email);

            var pedido = new PedidoUsuario { Email = email, MazoSeleccionado = mazo, Pokemons = pokemons };

            if (_context.PedidoUsuario == null)
                throw new InvalidOperationException("PedidoUsuario no está en DbContext.");

            _context.PedidoUsuario.Add(pedido);
            _context.SaveChanges();
        }

        public PedidoUsuario ObtenerPedidoUsuario(string email) =>
            string.IsNullOrWhiteSpace(email) || _context.PedidoUsuario == null
                ? throw new ArgumentException("El email no puede ser nulo ni vacío.")
                : _context.PedidoUsuario.Include(p => p.Pokemons).FirstOrDefault(p => p.Email == email);

        public void AgregarPokemonAFavoritos(string email, ProductoPokemon pokemon)
        {
            pokemon.Descripcion ??= "Sin descripción";
            pokemon.EnVenta = false;

            _context.ProductoPokemon.Add(pokemon);
            _context.SaveChanges();
        }

        public List<ProductoPokemon> ObtenerColeccionPokemon(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede ser nulo ni vacío.");

            return _context.ProductoPokemon
                .Include(p => p.Stats)
                .Where(p => p.Email == email)
                .ToList();
        }
    }
}
