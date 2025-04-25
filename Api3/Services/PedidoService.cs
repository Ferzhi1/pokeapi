using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using api3.Models;

namespace api3.Services
{
    public class PedidoService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly PokemonService _pokemonService; // ✅ Reutiliza datos de PokemonService

        public PedidoService(HttpClient httpClient, IMemoryCache cache, PokemonService pokemonService)
        {
            _httpClient = httpClient;
            _cache = cache;
            _pokemonService = pokemonService;
        }

        // 🟢 Obtener Pedido evitando consultas innecesarias a la API
        public async Task<PedidoPokemon> ObtenerPedidoAsync(string nombreMazo, string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreMazo) || string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("❌ ERROR: El nombre del mazo y el email son obligatorios.");
                    throw new ArgumentException("El nombre del mazo y el email son obligatorios.");
                }

                string cacheKey = $"Pedido_{email}_{nombreMazo}";

                // 🔥 Verificar primero en caché
                if (_cache.TryGetValue(cacheKey, out PedidoPokemon cachedPedido))
                {
                    Console.WriteLine($"✔ Pedido encontrado en caché ({cachedPedido.Pokemons?.Count} Pokémon(s)). No se consulta la API.");
                    return cachedPedido;
                }

                // 🛠️ Llamando a la API de Pokémon
                Console.WriteLine($"🔍 Consultando API para {nombreMazo}...");
                var pokemons = await _pokemonService.ObtenerPokemonsAsync(30);

                if (pokemons == null || !pokemons.Any())
                {
                    Console.WriteLine("❌ ERROR: La API no devolvió Pokémon. Verifica la conexión.");
                    throw new InvalidOperationException("No se pudieron obtener Pokémon de la API.");
                }

                Console.WriteLine($"🎴 Se obtuvieron {pokemons.Count} Pokémon(s) de la API.");

                var pedido = new PedidoPokemon(nombreMazo, 25.99m, email)
                {
                    Pokemons = pokemons
                };

                // 🔥 Cachea solo si los datos son válidos
                Console.WriteLine("💾 Guardando pedido en caché...");
                _cache.Set(cacheKey, pedido, TimeSpan.FromMinutes(30));

                return pedido;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en ObtenerPedidoAsync(): {ex.Message}");
                throw;
            }
        }

    }
}
