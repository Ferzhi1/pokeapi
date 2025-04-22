using api3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace api3.Services
{
    public class PokemonStorageService
    {
        private readonly Dictionary<string, PedidoUsuario> _storage = new();
        private readonly IMemoryCache _cache;
        private const string CacheKey = "coleccionPokemon";
        private readonly Dictionary<string, List<ProductoPokemon>> _coleccionPokemon = new();
        private readonly Dictionary<string, List<ProductoPokemon>> _ventaPokemon = new();

        public PokemonStorageService(IMemoryCache cache)
        {
            _cache = cache;
            ReiniciarCache(); // 🔄 Reinicia la caché en cada arranque
        }

        // ✅ Obtener Pedido de Usuario
        public PedidoUsuario ObtenerPedidoUsuario(string email)
        {
            Console.WriteLine($"🔎 Buscando pedido para {email}...");
            return _storage.TryGetValue(email, out var pedido) ? pedido : null;
        }

        // ✅ Guardar Pedido del Usuario
        public void GuardarPedidoUsuario(string email, string mazo, List<PedidoPokemon> pokemons)
        {
            Console.WriteLine($"💾 Guardando pedido para {email} con {pokemons.Count} Pokémon...");
            _storage[email] = new PedidoUsuario { MazoSeleccionado = mazo, Pokemons = pokemons };
        }

        // ✅ Agregar Pokémon a Favoritos
        public void AgregarPokemonAFavoritos(string email, ProductoPokemon pokemon)
        {
            Console.WriteLine($"🔍 Guardando Pokémon: {pokemon.Nombre}");

            if (!_coleccionPokemon.ContainsKey(email))
            {
                _coleccionPokemon[email] = new List<ProductoPokemon>();
            }

            _coleccionPokemon[email].Add(pokemon);

            Console.WriteLine($"✅ Pokémon {pokemon.Nombre} guardado en favoritos.");
        }

        // ✅ Obtener lista de Pokémon de la colección
        public List<ProductoPokemon> ObtenerColeccionPokemon(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("⚠️ Error: Email inválido.");
                return new List<ProductoPokemon>();
            }

            if (_cache.TryGetValue(email, out List<ProductoPokemon> coleccion) && coleccion != null)
            {
                Console.WriteLine($"📂 Colección obtenida desde caché para {email}: {coleccion.Count} Pokémon");
                return coleccion;
            }

            if (!_coleccionPokemon.ContainsKey(email))
            {
                Console.WriteLine($"⚠️ No se encontraron Pokémon en la colección para {email}.");
                return new List<ProductoPokemon>();
            }

            coleccion = _coleccionPokemon[email];

            _cache.Set(email, coleccion); // 🔥 Guardar en caché
            Console.WriteLine($"✅ Colección obtenida para {email}: {coleccion.Count} Pokémon");

            return coleccion;
        }

        // ✅ Agregar Pokémon a Venta
        public bool AgregarPokemonAVenta(string email, ProductoPokemon pokemon)
        {
            Console.WriteLine($"🛠 Recibido para venta: {pokemon.Nombre}");

            if (string.IsNullOrWhiteSpace(email) || pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre))
            {
                Console.WriteLine("⚠️ Error: Información inválida.");
                return false;
            }

            if (!_ventaPokemon.ContainsKey(email))
            {
                _ventaPokemon[email] = new List<ProductoPokemon>();
            }

            if (_ventaPokemon[email].Any(p => p.Nombre == pokemon.Nombre))
            {
                Console.WriteLine($"⚠️ Pokémon {pokemon.Nombre} ya está en la lista de venta.");
                return false;
            }

            _ventaPokemon[email].Add(pokemon);

            Console.WriteLine($"✅ Pokémon {pokemon.Nombre} agregado a la venta.");
            return true;
        }

        // ✅ Obtener lista de Pokémon en venta
        public List<ProductoPokemon> ObtenerPokemonEnVenta(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !_ventaPokemon.ContainsKey(email))
            {
                Console.WriteLine("⚠️ No hay Pokémon en venta para este usuario.");
                return new List<ProductoPokemon>();
            }

            Console.WriteLine($"📌 Obteniendo lista de Pokémon en venta para {email}...");
            return _ventaPokemon[email];
        }

        // ✅ Limpiar Caché
        public void LimpiarCache()
        {
            Console.WriteLine("🚀 Limpiando caché...");
            _cache.Remove(CacheKey);
            Console.WriteLine("✅ Caché reiniciada.");
        }

        // ✅ Reiniciar Caché al inicio
        private void ReiniciarCache()
        {
            Console.WriteLine("🔄 Reiniciando caché al inicio de la aplicación...");
            _cache.Set(CacheKey, new Dictionary<string, List<ProductoPokemon>>());
            Console.WriteLine("✅ Caché lista para almacenar datos.");
        }
    }
}
