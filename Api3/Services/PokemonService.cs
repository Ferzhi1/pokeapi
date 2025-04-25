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

        // 🌟 Obtener una lista aleatoria de Pokémon con caché
        public async Task<List<ProductoPokemon>> ObtenerPokemonsAsync(int cantidadPokemons)
        {
            List<ProductoPokemon> pokemons = new List<ProductoPokemon>();

            HashSet<int> idsAleatorios = new HashSet<int>(); // 🔥 Evita repetidos
            while (idsAleatorios.Count < cantidadPokemons)
            {
                idsAleatorios.Add(_random.Next(1, 898)); // ✅ Genera un ID aleatorio entre 1 y 898
            }

            foreach (var id in idsAleatorios)
            {
                try
                {
                    string cacheKey = $"Pokemon_{id}";
                    if (_cache.TryGetValue(cacheKey, out ProductoPokemon cachedPokemon))
                    {
                        pokemons.Add(cachedPokemon);
                        continue;
                    }

                    var detallesUrl = $"https://pokeapi.co/api/v2/pokemon/{id}";
                    var detallesResponse = await _httpClient.GetStringAsync(detallesUrl);
                    var detallesJson = JsonDocument.Parse(detallesResponse);

                    // ✅ Asegurar que Descripcion nunca sea NULL
                    string descripcion = detallesJson.RootElement.TryGetProperty("species", out var species)
                        ? species.GetProperty("name").GetString() ?? "Pokémon sin descripción"
                        : "Descripción no disponible";

                    var pokemon = new ProductoPokemon
                    {
                        Nombre = detallesJson.RootElement.GetProperty("name").GetString(),
                        ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{id}.png",
                        Descripcion = descripcion, // 🔥 Siempre se pasa una descripción válida
                        Precio = _random.Next(5, 100),
                        Rareza = _random.Next(1, 100) > 80 ? "Raro" : "Común",
                        Stats = detallesJson.RootElement.GetProperty("stats").EnumerateArray()
                            .Select(stat => new StatPokemon
                            {
                                Nombre = stat.GetProperty("stat").GetProperty("name").GetString(),
                                Valor = stat.GetProperty("base_stat").GetInt32()
                            }).ToList()
                    };

                    pokemons.Add(pokemon);
                    _cache.Set(cacheKey, pokemon, TimeSpan.FromMinutes(30)); // 🔥 Caché por más tiempo para evitar llamadas repetidas
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERROR al obtener Pokémon {id}: {ex.Message}");
                }
            }

            return pokemons;
        }
    }
}
