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
        private readonly Random _random = new();

        public PokemonService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<List<ProductoPokemon>> ObtenerPokemonsAsync(int cantidadPokemons)
        {
            var pokemons = new List<ProductoPokemon>();
            var idsAleatorios = new HashSet<int>();

            while (idsAleatorios.Count < cantidadPokemons)
                idsAleatorios.Add(_random.Next(1, 898));

            var tareas = idsAleatorios.Select(async id =>
            {
                string cacheKey = $"Pokemon_{id}";

                // 🚀 Verificar caché antes de llamar a la API
                if (_cache.TryGetValue(cacheKey, out ProductoPokemon cachedPokemon))
                {
                    Console.WriteLine($"✅ Recuperado desde caché: {cachedPokemon.Nombre}");
                    return cachedPokemon;
                }

                var respuesta = await ObtenerRespuestaConReintento(id);
                if (respuesta == null) return null;

                try
                {
                    var detallesJson = JsonDocument.Parse(respuesta);
                    var descripcion = detallesJson.RootElement.TryGetProperty("species", out var species)
                        ? species.GetProperty("name").GetString() ?? "Pokémon sin descripción"
                        : "Descripción no disponible";

                    var pokemon = new ProductoPokemon
                    {
                        Nombre = detallesJson.RootElement.GetProperty("name").GetString(),
                        ImagenUrl = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{id}.png",
                        Descripcion = descripcion,
                        Precio = _random.Next(5, 100),
                        Rareza = _random.Next(1, 100) > 80 ? "Raro" : "Común",
                        Stats = detallesJson.RootElement.GetProperty("stats").EnumerateArray()
                            .Select(stat => new StatPokemon
                            {
                                Nombre = stat.GetProperty("stat").GetProperty("name").GetString(),
                                Valor = stat.GetProperty("base_stat").GetInt32()
                            }).ToList()
                    };

                    // 🚀 Guardar en caché para evitar llamadas repetitivas
                    _cache.Set(cacheKey, pokemon, TimeSpan.FromMinutes(30));
                    return pokemon;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"❌ ERROR de JSON al procesar Pokémon {id}: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERROR inesperado al obtener Pokémon {id}: {ex.Message}");
                    return null;
                }
            });

            pokemons = (await Task.WhenAll(tareas)).Where(p => p is not null).ToList();

            // 🚀 Asegurar que siempre haya 50 Pokémon
            while (pokemons.Count < cantidadPokemons)
            {
                int nuevoId = _random.Next(1, 898);
                if (!pokemons.Any(p => p.Id == nuevoId))
                {
                    var nuevoPokemon = await ObtenerPokemonsAsync(1);
                    if (nuevoPokemon != null && nuevoPokemon.Any())
                    {
                        pokemons.Add(nuevoPokemon.First());
                    }
                }
            }

            Console.WriteLine($"📌 Total de Pokémon obtenidos: {pokemons.Count}");
            return pokemons;
        }

 
        private async Task<string> ObtenerRespuestaConReintento(int id)
        {
            int intentos = 0;
            while (intentos < 3) 
            {
                var respuesta = await _httpClient.GetStringAsync($"https://pokeapi.co/api/v2/pokemon/{id}");

                if (!string.IsNullOrWhiteSpace(respuesta))
                    return respuesta;

                intentos++;
                Console.WriteLine($"🔄 Reintentando obtener Pokémon {id} ({intentos}/3)");
                await Task.Delay(500);
            }

            Console.WriteLine($"❌ No se pudo obtener Pokémon {id} después de 3 intentos");
            return null;
        }
    }
}
