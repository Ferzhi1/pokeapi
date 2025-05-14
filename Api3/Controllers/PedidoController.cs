using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using api3.Services;
using api3.Models;
using Microsoft.Extensions.Caching.Memory;

namespace api3.Controllers
{
    [Route("Pedido")]
    public class PedidoController : Controller
    {
        private readonly PokemonService _pokemonService;
        private readonly CheckoutService _checkoutService;
        private readonly PokemonStorageService _pokemonStorageService;
        private readonly IMemoryCache _cache; // 🔥 Agregamos caché

        public PedidoController(PokemonService pokemonService, CheckoutService checkoutService, PokemonStorageService pokemonStorageService, IMemoryCache cache)
        {
            _pokemonService = pokemonService;
            _checkoutService = checkoutService;
            _pokemonStorageService = pokemonStorageService;
            _cache = cache;
        }

        private static string ObtenerImagenUrl(string nombreMazo) => nombreMazo switch
        {
            "Mazo Pequeño" => "/img/mazo1.jpg",
            "Mazo Mediano" => "/img/mazo2.jpg",
            "Mazo Grande" => "/img/mazo6.jpg",
            _ => "/img/default.jpg"
        };

        [HttpGet("Confirmacion")]
        public async Task<IActionResult> Confirmacion(string nombreMazo, string email)
        {
            if (string.IsNullOrWhiteSpace(nombreMazo) || string.IsNullOrWhiteSpace(email))
                return BadRequest("El nombre del mazo y el email son obligatorios.");

            var usuarioPokemon = _pokemonStorageService.ObtenerUsuarioPokemon(email);
            if (usuarioPokemon == null)
                return BadRequest("El usuario no existe en la base de datos.");

            Console.WriteLine($"📌 Valor recibido de nombreMazo: {nombreMazo}");

            var cantidadPokemons = nombreMazo.Trim().ToLower() switch
            {
                "mazo pequeño" => 30,
                "mazo mediano" => 40,
                "mazo grande" => 50,
                _ => 30
            };

            // 🚀 Usar caché antes de llamar a la API
            string cacheKey = $"PokemonMazo_{cantidadPokemons}";
            if (!_cache.TryGetValue(cacheKey, out List<ProductoPokemon> pokemons))
            {
                Console.WriteLine($"⚠️ Mazo no encontrado en caché, llamando a la API...");
                pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

                if (pokemons == null || !pokemons.Any())
                    return BadRequest("No se encontraron Pokémon para este mazo.");

                _cache.Set(cacheKey, pokemons, TimeSpan.FromMinutes(30));
            }
            else
            {
                Console.WriteLine($"✅ Recuperado desde caché: {cacheKey}");
            }

            var pedidoPokemon = new PedidoPokemon(nombreMazo, 25.99m, email)
            {
                Pokemons = pokemons
            };

            return View("~/Views/Pedido/Confirmacion.cshtml", pedidoPokemon);
        }

        [HttpGet("Checkout")]
        public async Task<IActionResult> Checkout(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return RedirectToAction("Index");

            var imagenUrl = ObtenerImagenUrl(nombre);
            nombre = nombre.Trim().ToLower();

            var cantidadPokemons = nombre switch
            {
                "mazo pequeño" => 30,
                "mazo mediano" => 40,
                "mazo grande" => 50,
                _ => 30
            };

            var precioFijo = nombre switch
            {
                "mazo pequeño" => 25.99m,
                "mazo mediano" => 40.99m,
                "mazo grande" => 50.99m,
                _ => 25.99m
            };

            // 🚀 Usar caché antes de llamar a la API
            string cacheKey = $"PokemonMazo_{cantidadPokemons}";
            if (!_cache.TryGetValue(cacheKey, out List<ProductoPokemon> pokemons))
            {
                Console.WriteLine($"⚠️ Mazo no encontrado en caché, llamando a la API...");
                pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

                if (pokemons == null || !pokemons.Any())
                    return BadRequest("No se encontraron Pokémon para este mazo.");

                _cache.Set(cacheKey, pokemons, TimeSpan.FromMinutes(30));
            }
            else
            {
                Console.WriteLine($"✅ Recuperado desde caché: {cacheKey}");
            }

            var mazo = new MazoPokemon(nombre, precioFijo, imagenUrl)
            {
                Pokemons = pokemons
            };

            return View(mazo);
        }
    }
}
