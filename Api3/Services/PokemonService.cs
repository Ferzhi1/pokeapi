using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using api3.Models;

namespace api3.Services
{
    public class PokemonService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly Random _random = new Random();

        public PokemonService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        // 🔎 Obtener una lista de Pokémon desde la PokéAPI con sus estadísticas
        public async Task<List<ProductoPokemon>> ObtenerPokemonsAsync(int cantidadPokemons)
        {
            if (_cache.TryGetValue($"ListaPokemons_{cantidadPokemons}", out List<ProductoPokemon> cachedPokemons))
                return cachedPokemons;

            var url = $"https://pokeapi.co/api/v2/pokemon?limit={cantidadPokemons}";
            var response = await _httpClient.GetStringAsync(url);
            var jsonData = JsonDocument.Parse(response);

            var pokemons = new List<ProductoPokemon>();
            var listaTemp = jsonData.RootElement.GetProperty("results").EnumerateArray().ToList();

            foreach (var pokemon in listaTemp)
            {
                var nombre = pokemon.GetProperty("name").GetString();
                var id = pokemon.GetProperty("url").GetString().Split('/')[6];

                // 🌟 Obtener detalles del Pokémon, incluyendo sus estadísticas
                var detallesUrl = $"https://pokeapi.co/api/v2/pokemon/{id}";
                var detallesResponse = await _httpClient.GetStringAsync(detallesUrl);
                var detallesJson = JsonDocument.Parse(detallesResponse);

                var stats = detallesJson.RootElement.GetProperty("stats").EnumerateArray()
                    .Select(stat => new StatPokemon
                    {
                        Nombre = stat.GetProperty("stat").GetProperty("name").GetString(),
                        Valor = stat.GetProperty("base_stat").GetInt32()
                    }).ToList();

                pokemons.Add(new ProductoPokemon
                {
                    Nombre = nombre,
                    ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{id}.png",
                    Descripcion = "Pokémon obtenido de la PokéAPI",
                    Precio = _random.Next(5, 100), // ✅ Asigna un precio aleatorio
                    Rareza = _random.Next(1, 100) > 80 ? "Raro" : "Común", // ✅ Simulación de rareza aleatoria
                    Stats = stats // ✅ Agrega las estadísticas obtenidas
                });
            }

            _cache.Set($"ListaPokemons_{cantidadPokemons}", pokemons, TimeSpan.FromHours(1));
            return pokemons;
        }
    }
}
