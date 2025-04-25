using System;
using api3.Services;
using api3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace api3.Controllers
{
    [Route("Pedido")]
    public class PedidoController : Controller
    {
        private readonly PokemonService _pokemonService;
        private readonly CheckoutService _checkoutService;
        private readonly PokemonStorageService _pokemonStorageService;

        public PedidoController(PokemonService pokemonService, CheckoutService checkoutService, PokemonStorageService pokemonStorageService)
        {
            _pokemonService = pokemonService;
            _checkoutService = checkoutService;
            _pokemonStorageService = pokemonStorageService;
        }

        private string ObtenerImagenUrl(string nombreMazo)
        {
            return nombreMazo switch
            {
                "Mazo Pequeño" => "/img/mazo1.jpg",
                "Mazo Mediano" => "/img/mazo2.jpg",
                "Mazo Grande" => "/img/mazo3.jpg",
                _ => "/img/default.jpg"
            };
        }

        [HttpGet("Confirmacion")]
        public async Task<IActionResult> Confirmacion(string nombreMazo, string email)
        {
            if (string.IsNullOrWhiteSpace(nombreMazo) || string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("❌ ERROR: El nombre del mazo y el email son obligatorios.");
                return BadRequest("El nombre del mazo y el email son obligatorios.");
            }

            Console.WriteLine($"🔍 Buscando usuario con email: {email}");
            var usuarioPokemon = _pokemonStorageService.ObtenerUsuarioPokemon(email);

            if (usuarioPokemon == null)
            {
                Console.WriteLine("❌ ERROR: Usuario no encontrado en la base de datos.");
                return BadRequest("El usuario no existe en la base de datos.");
            }
            Console.WriteLine($"✅ Usuario encontrado: {usuarioPokemon.Email}");

            var imagenUrl = ObtenerImagenUrl(nombreMazo);
            var cantidadPokemons = nombreMazo switch
            {
                "Mazo Pequeño" => 30,
                "Mazo Mediano" => 40,
                "Mazo Grande" => 50,
                _ => 30
            };

            Console.WriteLine($"🔍 Obteniendo {cantidadPokemons} Pokémon(s) para mostrar.");
            var pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

            if (pokemons == null || !pokemons.Any())
            {
                Console.WriteLine("❌ Error: No se pudieron obtener Pokémon.");
                return BadRequest("No se encontraron Pokémon para este mazo.");
            }

            var pedidoPokemon = new PedidoPokemon(nombreMazo, 25.99m, email)
            {
                Pokemons = pokemons
            };

            // ✅ Ahora estamos pasando un objeto del tipo correcto a la vista
            Console.WriteLine($"✅ Pedido listo con {pedidoPokemon.Pokemons.Count} Pokémon(s) para mostrar, pero no guardar.");

            return View("~/Views/Pedido/Confirmacion.cshtml", pedidoPokemon);
        }


        public async Task<IActionResult> Checkout(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return RedirectToAction("Index");
            }

            var imagenUrl = ObtenerImagenUrl(nombre);

            nombre = nombre.Trim().ToLower();
            var cantidadPokemons = nombre switch
            {
                "mazo pequeño" => 30,
                "mazo mediano" => 40,
                "mazo grande" => 50,
                _ => 30
            };

            Console.WriteLine($"🔍 Cargando {cantidadPokemons} Pokémon(s) para el mazo: {nombre}");
            var pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

            if (pokemons == null || !pokemons.Any())
            {
                Console.WriteLine("❌ Error: No se pudieron obtener Pokémon.");
                return BadRequest("No se encontraron Pokémon para este mazo.");
            }

            var mazo = new MazoPokemon(nombre, 25.99m, imagenUrl)
            {
                Pokemons = pokemons
            };

            Console.WriteLine($"✅ Mazo listo con {mazo.Pokemons.Count} Pokémon(s).");

            return View(mazo);
        }
    }
}
