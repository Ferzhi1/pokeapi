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
            if (string.IsNullOrWhiteSpace(nombreMazo) || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El nombre del mazo y el email son obligatorios.");

            string cacheKey = $"Pedido_{email}_{nombreMazo}";

            // 🔥 Verificar primero en caché
            if (_cache.TryGetValue(cacheKey, out PedidoPokemon cachedPedido))
            {
                Console.WriteLine("✔ Pedido encontrado en caché. No se consulta la API.");
                return cachedPedido;
            }

            // 🟢 Consultar en `PokemonService`
            var pokemons = await _pokemonService.ObtenerPokemonsAsync(30);

            var pedido = new PedidoPokemon(nombreMazo, 25.99m, email)
            {
                Pokemons = pokemons
            };

            _cache.Set(cacheKey, pedido, TimeSpan.FromMinutes(30)); // ✅ Cachea el pedido por 30 min

            return pedido;
        }
    }
}
