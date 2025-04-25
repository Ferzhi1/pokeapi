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
            try
            {
                Console.WriteLine($"📩 Recibido: Email = {email}, Mazo = {mazo}, Pokémon(s) = {pokemons?.Count}");

                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("❌ ERROR: Email no puede ser nulo ni vacío.");
                    throw new ArgumentException("El email no puede ser nulo ni vacío.");
                }

                if (string.IsNullOrWhiteSpace(mazo))
                {
                    Console.WriteLine("❌ ERROR: El nombre del mazo no puede ser nulo.");
                    throw new ArgumentException("El nombre del mazo no puede ser nulo.");
                }

                if (pokemons == null || pokemons.Count == 0)
                {
                    Console.WriteLine("❌ ERROR: La lista de Pokémon no puede estar vacía.");
                    throw new ArgumentException("La lista de Pokémon no puede estar vacía.");
                }

                Console.WriteLine($"🔍 Asignando EmailUsuario a {pokemons.Count} Pokémon(s): {email}");

                foreach (var pokemon in pokemons)
                {
                    if (pokemon == null)
                    {
                        Console.WriteLine("❌ ERROR: Se encontró un Pokémon nulo en la lista.");
                        throw new ArgumentException("Se encontró un Pokémon nulo en la lista.");
                    }

                    pokemon.UsuarioEmail = email;  // ✅ Asigna el email antes de insertar
                    Console.WriteLine($"✅ Asignado email a Pokémon: {pokemon.NombreMazo}");
                }

                // ✅ Crear y guardar el pedido
                var pedido = new PedidoUsuario { Email = email, MazoSeleccionado = mazo, Pokemons = pokemons };

                Console.WriteLine("💾 Verificando contexto de base de datos...");
                if (_context.PedidoUsuario == null)
                {
                    Console.WriteLine("❌ ERROR: La tabla PedidoUsuario no está disponible en DbContext.");
                    throw new InvalidOperationException("PedidoUsuario no está en DbContext.");
                }

                _context.PedidoUsuario.Add(pedido);

                Console.WriteLine("💾 Ejecutando SaveChanges()...");
                _context.SaveChanges();

                Console.WriteLine($"✅ Pedido guardado correctamente en la base de datos: {mazo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en GuardarPedidoUsuario(): {ex.Message}");
                throw;
            }
        }

        public PedidoUsuario ObtenerPedidoUsuario(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("❌ ERROR: El email no puede ser nulo ni vacío.");
                throw new ArgumentException("El email no puede ser nulo ni vacío.");
            }

            Console.WriteLine($"🔍 Buscando pedido en PedidoUsuario para el email: {email}");

            try
            {
                // ✅ Verificamos si el DbSet PedidoUsuario está disponible
                if (_context.PedidoUsuario == null)
                {
                    Console.WriteLine("❌ ERROR: La tabla PedidoUsuario no está disponible en DbContext.");
                    throw new InvalidOperationException("PedidoUsuario no está en DbContext.");
                }

                // 🔹 Recuperando el pedido con los Pokémon relacionados
                var pedido = _context.PedidoUsuario
                    .Include(p => p.Pokemons) // ✅ Si `PedidoUsuario` tiene relación con `PedidoPokemon`
                    .FirstOrDefault(p => p.Email == email);

                if (pedido == null)
                {
                    Console.WriteLine("❌ No se encontró pedido para el usuario.");
                    return null;
                }

                // ✅ Verificar la cantidad de Pokémon recuperados
                Console.WriteLine($"✅ Pedido encontrado: {pedido.MazoSeleccionado} con {pedido.Pokemons?.Count} Pokémon(s).");

                return pedido;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en ObtenerPedidoUsuario(): {ex.Message}");
                throw;
            }
        }




        // ✅ Agregar Pokémon a Favoritos
        public void AgregarPokemonAFavoritos(string email, ProductoPokemon pokemon)
        {
            try
            {
                // 🔥 Asegurar que la descripción nunca sea NULL
                pokemon.Descripcion = string.IsNullOrWhiteSpace(pokemon.Descripcion) ? "Sin descripción" : pokemon.Descripcion;

                _context.ProductoPokemon.Add(pokemon);
                _context.SaveChanges();
                Console.WriteLine("✅ Pokémon guardado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en AgregarPokemonAFavoritos(): {ex.Message}");
                throw;
            }
        }



        // ✅ Obtener lista de Pokémon de la colección
        // ✅ Obtener lista de Pokémon de la colección con estadísticas
        public List<ProductoPokemon> ObtenerColeccionPokemon(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("❌ Error: El email no puede ser nulo ni vacío.");
                    throw new ArgumentException("El email no puede ser nulo ni vacío.");
                }

                Console.WriteLine($"🔍 Buscando colección de Pokémon para el usuario: {email}");

                var pokemonsGuardados = _context.ProductoPokemon
                    .Include(p => p.Stats) // 🔥 Asegura que se cargan las estadísticas
                    .Where(p => p.Email == email)
                    .ToList();

                if (!pokemonsGuardados.Any())
                {
                    Console.WriteLine("⚠ No se encontraron Pokémon en la colección.");
                }
                else
                {
                    Console.WriteLine($"✅ Enviando colección con {pokemonsGuardados.Count} Pokémon(s).");
                }

                return pokemonsGuardados;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en ObtenerColeccionPokemon(): {ex.Message}");
                return new List<ProductoPokemon>(); // Retorna una lista vacía en caso de error
            }
        }


    }
}


