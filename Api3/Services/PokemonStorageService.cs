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

        public PokemonStorageService(ApplicationDbContext context)
        {
            _context = context;
        }

        //🍭  Obtener usuario desde UsuariosPokemonApi
        public UsuariosPokemonApi ObtenerUsuarioPokemon(string email)
        {
            Console.WriteLine($"🟢 Método ObtenerUsuarioPokemon() ha sido llamado con email: {email}");

            var usuario = _context.UsuariosPokemonApi.FirstOrDefault(u => u.Email == email);

            Console.WriteLine(usuario != null
                ? $"✅ Usuario encontrado en la base de datos: {usuario.Email}"
                : "❌ Usuario NO encontrado en la base de datos.");

            return usuario;
        }

        public string ObtenerEmailUsuario(string email)
        {
            Console.WriteLine($"🔍 Intentando acceder a la base de datos para obtener el email: {email}");

            var usuarioPokemon = _context.UsuariosPokemonApi
                .FirstOrDefault(u => u.Email.Trim().ToLower() == email.Trim().ToLower());

            Console.WriteLine(usuarioPokemon != null
                ? $"✅ Usuario encontrado con email: {usuarioPokemon.Email}"
                : "❌ Usuario NO encontrado en la base de datos.");

            if (usuarioPokemon != null)
            {
                return usuarioPokemon.Email;
            }

            throw new InvalidOperationException("El usuario no existe en la base de datos.");
        }



        // ✅ Obtener Pedido de Usuario desde la base de datos
        public void GuardarPedidoUsuario(string email, string mazo, List<PedidoPokemon> pokemons)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("⚠ ERROR: El email no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(mazo))
                throw new ArgumentException("⚠ ERROR: El nombre del mazo no puede ser nulo.");

            if (pokemons == null || pokemons.Count == 0)
                throw new ArgumentException("⚠ ERROR: La lista de Pokémon no puede estar vacía.");

            Console.WriteLine($"🔍 Asignando EmailUsuario a {pokemons.Count} Pokémon(s): {email}");

            foreach (var pokemon in pokemons)
            {
                if (pokemon == null)
                    throw new ArgumentException("⚠ ERROR: Se encontró un Pokémon nulo en la lista.");

                pokemon.UsuarioEmail = email;  // ✅ Asigna el email antes de insertar
            }

            var pedido = new PedidoUsuario { Email = email, MazoSeleccionado = mazo, Pokemons = pokemons };

            try
            {
                _context.PedidoUsuario.Add(pedido);
                _context.SaveChanges();
                Console.WriteLine($"✅ Pedido guardado correctamente en la base de datos: {mazo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR al guardar el pedido: {ex.Message}");
                throw;
            }
        }
        public PedidoUsuario ObtenerPedidoUsuario(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("⚠ ERROR: El email no puede ser nulo ni vacío.");

            Console.WriteLine($"🔍 Buscando pedido en PedidoUsuario para el email: {email}");

            var pedido = _context.PedidoUsuario
                .Include(p => p.Pokemons) // ✅ Si `PedidoUsuario` tiene relación con `PedidoPokemon`
                .FirstOrDefault(p => p.Email == email);

            Console.WriteLine(pedido != null
                ? $"✅ Pedido encontrado: {pedido.MazoSeleccionado}"
                : "❌ No se encontró pedido para el usuario.");

            return pedido;
        }



        // ✅ Agregar Pokémon a Favoritos
        public void AgregarPokemonAFavoritos(string email, ProductoPokemon pokemon)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede ser nulo ni vacío.");
            if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre))
                throw new ArgumentException("Los datos del Pokémon son inválidos.");

            pokemon.Email = email;
            _context.ProductoPokemon.Add(pokemon);
            _context.SaveChanges();
        }

        // ✅ Obtener lista de Pokémon de la colección
        public List<ProductoPokemon> ObtenerColeccionPokemon(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede ser nulo ni vacío.");

            return _context.ProductoPokemon
                .Where(p => p.Email == email)
                .ToList();
        }

        // ✅ Agregar Pokémon a Venta
        public bool AgregarPokemonAVenta(string email, ProductoPokemon pokemon)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede ser nulo ni vacío.");
            if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre))
                throw new ArgumentException("Los datos del Pokémon son inválidos.");

            if (_context.ProductoPokemon.Any(p => p.Nombre == pokemon.Nombre && p.Email == email))
            {
                return false; // Ya existe en la venta
            }

            pokemon.Email = email;
            pokemon.EnVenta = true;
            _context.ProductoPokemon.Add(pokemon);
            _context.SaveChanges();
            return true;
        }

        // ✅ Obtener lista de Pokémon en venta
        public List<ProductoPokemon> ObtenerPokemonEnVenta(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede ser nulo ni vacío.");

            return _context.ProductoPokemon
                .Where(p => p.Email == email && p.EnVenta)
                .ToList();
        }
    }
}
