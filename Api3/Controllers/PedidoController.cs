using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using api3.Services;
using api3.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

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

        [HttpPost("Confirmacion")]
        public async Task<IActionResult> Confirmacion([FromForm] string nombreMazo)
        {
            Console.WriteLine("Método Confirmacion iniciado");
            Console.WriteLine($"✅ Nombre del mazo recibido: {nombreMazo}");

            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine("El usuario no está autenticado.");
                return Unauthorized("El usuario no está autenticado.");
            }

            var emailUsuarioAutenticado = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            Console.WriteLine($"Email obtenido: {emailUsuarioAutenticado}");

            if (string.IsNullOrWhiteSpace(emailUsuarioAutenticado))
            {
                Console.WriteLine("No se pudo obtener el email del usuario autenticado.");
                return BadRequest("No se pudo obtener el email del usuario autenticado.");
            }

            var usuarioPokemon = _pokemonStorageService.ObtenerUsuarioPokemon(emailUsuarioAutenticado);
            Console.WriteLine($"Usuario obtenido: {usuarioPokemon?.Nombre}");

            if (usuarioPokemon == null)
            {
                Console.WriteLine("El usuario no existe en la base de datos.");
                return BadRequest("El usuario no existe en la base de datos.");
            }

            if (!usuarioPokemon.CorreoValidado)
            {
                Console.WriteLine("El correo no ha sido validado.");
                return BadRequest("Debes validar tu correo electrónico antes de proceder con la compra.");
            }

            if (string.IsNullOrWhiteSpace(nombreMazo))
            {
                Console.WriteLine("❌ Error: El nombre del mazo es obligatorio.");
                return BadRequest("El nombre del mazo es obligatorio.");
            }

            var cantidadPokemons = nombreMazo.Trim().ToLower() switch
            {
                "mazo pequeño" => 30,
                "mazo mediano" => 40,
                "mazo grande" => 50,
                _ => 30
            };

            Console.WriteLine($"Cantidad de Pokémon solicitados: {cantidadPokemons}");

            string cacheKey = $"PokemonMazo_{cantidadPokemons}";
            if (!_cache.TryGetValue(cacheKey, out List<ProductoPokemon> pokemons))
            {
                Console.WriteLine("No se encontró en caché, obteniendo Pokémon desde el servicio...");
                pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

                if (pokemons == null || !pokemons.Any())
                {
                    Console.WriteLine("No se encontraron Pokémon para este mazo.");
                    return BadRequest("No se encontraron Pokémon para este mazo.");
                }

                _cache.Set(cacheKey, pokemons, TimeSpan.FromMinutes(30));
                Console.WriteLine("Pokémon almacenados en caché.");
            }
            else
            {
                Console.WriteLine("Pokémon obtenidos desde caché.");
            }

            var pedidoPokemon = new PedidoPokemon(nombreMazo, 25.99m, emailUsuarioAutenticado)
            {
                Pokemons = pokemons
            };

            Console.WriteLine($"Pedido creado para {emailUsuarioAutenticado}");

            return View("~/Views/Pedido/Confirmacion.cshtml", pedidoPokemon);
        }




        [HttpGet("Checkout")]
        public async Task<IActionResult> Checkout(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                Console.WriteLine("❌ El nombre del mazo es obligatorio.");
                return RedirectToAction("Index");
            }

            var imagenUrl = ObtenerImagenUrl(nombre);
            if (string.IsNullOrWhiteSpace(imagenUrl))
            {
                Console.WriteLine("❌ No se encontró una imagen para el mazo.");
                return BadRequest("No se encontró una imagen para el mazo.");
            }

            nombre = nombre.Trim().ToLower();

            var cantidadPokemons = nombre switch
            {
                "mazo pequeño" => 30,
                "mazo mediano" => 40,
                "mazo grande" => 50,
                _ => 30
            };

            if (cantidadPokemons == 30 && nombre != "mazo pequeño" && nombre != "mazo mediano" && nombre != "mazo grande")
            {
                Console.WriteLine($"⚠️ Mazo desconocido: {nombre}. Se asignaron 30 Pokémon por defecto.");
            }

            var precioFijo = nombre switch
            {
                "mazo pequeño" => 25.99m,
                "mazo mediano" => 40.99m,
                "mazo grande" => 50.99m,
                _ => 25.99m
            };

            string cacheKey = $"PokemonMazo_{cantidadPokemons}";
            Console.WriteLine($"🔍 Buscando en caché: {cacheKey}");

            if (!_cache.TryGetValue(cacheKey, out List<ProductoPokemon> pokemons))
            {
                Console.WriteLine("❌ No se encontró en caché, obteniendo Pokémon desde el servicio...");
                pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

                if (pokemons == null || !pokemons.Any())
                {
                    Console.WriteLine("❌ No se encontraron Pokémon para este mazo.");
                    return BadRequest("No se encontraron Pokémon para este mazo.");
                }

                _cache.Set(cacheKey, pokemons, TimeSpan.FromMinutes(30));
                Console.WriteLine("✅ Pokémon almacenados en caché.");
            }
            else
            {
                Console.WriteLine($"✅ Recuperado desde caché: {cacheKey}");
            }

            Console.WriteLine($"✅ Total de Pokémon obtenidos: {pokemons?.Count}");

            var mazo = new MazoPokemon(nombre, precioFijo, imagenUrl)
            {
                Pokemons = pokemons
            };

            return View(mazo);
        }

    }
}
