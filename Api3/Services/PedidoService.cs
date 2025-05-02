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
        private readonly PokemonService _pokemonService;

        public PedidoService(HttpClient httpClient, IMemoryCache cache, PokemonService pokemonService)
        {
            _httpClient = httpClient;
            _cache = cache;
            _pokemonService = pokemonService;
        }

        public async Task<PedidoPokemon> ObtenerPedidoAsync(string nombreMazo, string email)
        {
            if (string.IsNullOrWhiteSpace(nombreMazo) || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El nombre del mazo y el email son obligatorios.");

            string cacheKey = $"Pedido_{email}_{nombreMazo}";

            if (_cache.TryGetValue(cacheKey, out PedidoPokemon cachedPedido))
                return cachedPedido;

            var pokemons = await _pokemonService.ObtenerPokemonsAsync(30);
            if (pokemons is not { Count: > 0 })
                throw new InvalidOperationException("No se pudieron obtener Pokémon de la API.");

            var pedido = new PedidoPokemon(nombreMazo, 25.99m, email)
            {
                Pokemons = pokemons
            };

            _cache.Set(cacheKey, pedido, TimeSpan.FromMinutes(30));
            return pedido;
        }
    }
}
