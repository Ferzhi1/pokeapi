using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using api3.Models;

namespace api3.Services
{
    public class PokemonStorageService
    {
        private readonly Dictionary<string, PedidoUsuario> _storage = new();
        private readonly string _archivoJson = "favoritos.json"; // 📂 Archivo de almacenamiento
        private Dictionary<string, List<ProductoPokemon>> _coleccionPokemon = new();
        private readonly List<ProductoPokemon> _pokemonsEnVenta = new List<ProductoPokemon>();

        public PokemonStorageService()
        {
            Console.WriteLine("🔄 Iniciando carga de datos desde archivo...");
            CargarDatosDesdeArchivo();
            Console.WriteLine("✅ Carga de datos completada.");
        }

        public PedidoUsuario ObtenerPedidoUsuario(string email)
        {
            Console.WriteLine($"🔎 Buscando pedido para {email}...");
            return _storage.TryGetValue(email, out var pedido) ? pedido : null;
        }

        public void GuardarPedidoUsuario(string email, string mazo, List<PedidoPokemon> pokemons)
        {
            Console.WriteLine($"💾 Guardando pedido para {email} con {pokemons.Count} Pokémon...");
            _storage[email] = new PedidoUsuario { MazoSeleccionado = mazo, Pokemons = pokemons };
        }

        // ✅ Agregar Pokémon a favoritos con almacenamiento persistente
        public void AgregarPokemonAFavoritos(string email, ProductoPokemon pokemon)
        {
            Console.WriteLine($"🛠 Antes de guardar en favoritos/venta: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");

            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("⚠️ Error: Email inválido.");
                return;
            }

            if (!_coleccionPokemon.ContainsKey(email))
            {
                _coleccionPokemon[email] = new List<ProductoPokemon>();
            }

            // 🛡️ Evitar duplicados
            if (_coleccionPokemon[email].Any(p => p.Nombre == pokemon.Nombre))
            {
                Console.WriteLine($"ℹ️ Pokémon {pokemon.Nombre} ya está en la colección de {email}.");
                return;
            }

            // ✅ Verificar si `ImagenUrl` está presente
            if (string.IsNullOrEmpty(pokemon.ImagenUrl))
            {
                Console.WriteLine($"⚠️ Advertencia: Pokémon {pokemon.Nombre} no tiene URL de imagen.");
            }
            else
            {
                Console.WriteLine($"🖼️ URL de imagen asignada: {pokemon.ImagenUrl}");
            }

            Console.WriteLine($"✅ Pokémon agregado a la colección de {email}: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");
            pokemon.Rareza = string.IsNullOrWhiteSpace(pokemon.Rareza) ? "Desconocida" : pokemon.Rareza; // 🔥 Evita rareza vacía
            _coleccionPokemon[email].Add(pokemon);

            pokemon.Rareza = string.IsNullOrWhiteSpace(pokemon.Rareza) ? "Desconocida" : pokemon.Rareza;


            GuardarDatosEnArchivo(); // 🔥 Guardar en JSON después de agregar
        }

        public List<ProductoPokemon> ObtenerColeccionPokemon(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("⚠️ Error: Email inválido.");
                return new List<ProductoPokemon>();
            }

            if (!_coleccionPokemon.TryGetValue(email, out var lista))
            {
                Console.WriteLine($"❌ No se encontró una colección para el email {email}.");
                return new List<ProductoPokemon>();
            }

            Console.WriteLine($"🔍 Colección obtenida para {email}: {lista.Count} Pokémon");
            foreach (var pokemon in lista)
            {
                Console.WriteLine($"🐉 Pokémon en colección: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");
            }

            return lista;
        }

        // ✅ Guardar colección en archivo JSON
        private void GuardarDatosEnArchivo()
        {
            Console.WriteLine("💾 Guardando datos en JSON...");
            var jsonData = JsonSerializer.Serialize(_coleccionPokemon, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_archivoJson, jsonData);
            foreach (var usuario in _coleccionPokemon)
            {
                foreach (var pokemon in usuario.Value)
                {
                    pokemon.Rareza = string.IsNullOrWhiteSpace(pokemon.Rareza) ? "Desconocida" : pokemon.Rareza;
                    Console.WriteLine($"📝 Guardando Pokémon: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");
                }
            }

            Console.WriteLine("✅ Datos guardados correctamente.");
        }

        // ✅ Cargar colección desde archivo JSON al iniciar la app
        private void CargarDatosDesdeArchivo()
        {
            foreach (var usuario in _coleccionPokemon)
            {
                foreach (var pokemon in usuario.Value)
                {
                    pokemon.Rareza = string.IsNullOrWhiteSpace(pokemon.Rareza) ? "Desconocida" : pokemon.Rareza; // 🔥 Evita rareza vacía al cargar
                    Console.WriteLine($"🔎 Pokémon: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");
                }
            }

            if (File.Exists(_archivoJson))
            {
                Console.WriteLine("📂 Cargando datos desde archivo JSON...");
                string jsonData = File.ReadAllText(_archivoJson);
                _coleccionPokemon = JsonSerializer.Deserialize<Dictionary<string, List<ProductoPokemon>>>(jsonData) ?? new();

                Console.WriteLine("✅ Datos cargados con éxito. Colección contiene:");
                foreach (var usuario in _coleccionPokemon)
                {
                    Console.WriteLine($"📌 Usuario: {usuario.Key}, Total Pokémon: {usuario.Value.Count}");
                    foreach (var pokemon in usuario.Value)
                    {
                        Console.WriteLine($"🔎 Pokémon: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");
                    }
                }
            }
            else
            {
                Console.WriteLine("⚠️ No se encontró el archivo JSON, se crea uno nuevo.");
                _coleccionPokemon = new();
            }
        }
    }
}
