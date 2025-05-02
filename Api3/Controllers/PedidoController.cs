using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using api3.Services;
using api3.Models;

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

            var cantidadPokemons = nombreMazo switch
            {
                "Mazo Pequeño" => 30,
                "Mazo Mediano" => 40,
                "Mazo Grande" => 50,
                _ => 30
            };

            var pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);
            if (pokemons == null || !pokemons.Any())
                return BadRequest("No se encontraron Pokémon para este mazo.");

            var pedidoPokemon = new PedidoPokemon(nombreMazo, 25.99m, email)
            {
                Pokemons = pokemons
            };

            return View("~/Views/Pedido/Confirmacion.cshtml", pedidoPokemon);
        }

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

            var pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);
            if (pokemons == null || !pokemons.Any())
                return BadRequest("No se encontraron Pokémon para este mazo.");

            var mazo = new MazoPokemon(nombre, 25.99m, imagenUrl)
            {
                Pokemons = pokemons
            };

            return View(mazo);
        }
    }
}
