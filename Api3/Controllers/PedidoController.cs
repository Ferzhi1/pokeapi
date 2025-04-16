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
                return BadRequest("El nombre del mazo y el email son obligatorios.");
            }

            var cantidadPokemons = nombreMazo switch
            {
                "Mazo Pequeño" => 30,
                "Mazo Mediano" => 40,
                "Mazo Grande" => 50,
                _ => 30
            };

            var imagenUrl = ObtenerImagenUrl(nombreMazo);
            var mazo = new MazoPokemon(nombreMazo, 25.99m, imagenUrl);

            // 🟢 Verificar si el usuario ya tiene un pedido guardado
            var pedidoUsuario = _pokemonStorageService.ObtenerPedidoUsuario(email);

            if (pedidoUsuario == null || pedidoUsuario.MazoSeleccionado != nombreMazo)
            {
                var nuevosPokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

              
                var nuevoPedidoPokemon = new PedidoPokemon(nombreMazo, mazo.Precio, email)
                {
                    Pokemons = nuevosPokemons
                };

                _pokemonStorageService.GuardarPedidoUsuario(email, nombreMazo, new List<PedidoPokemon> { nuevoPedidoPokemon });

                // ✅ Asignar los Pokémon obtenidos al mazo
                mazo.Pokemons = nuevosPokemons;
            }
            else
            {
                mazo.Pokemons = pedidoUsuario.Pokemons.Select(p => new ProductoPokemon
                {
                    Nombre = p.NombreMazo,
                    Precio = p.Precio,
                    ImagenUrl = imagenUrl
                    
                }).ToList();
            }

            var pedidoPokemon = new PedidoPokemon(nombreMazo, mazo.Precio, email)
            {
                Pokemons = mazo.Pokemons
            };

            return View("~/Views/Pedido/Confirmacion.cshtml", pedidoPokemon);


           
        }
        public IActionResult Checkout(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return RedirectToAction("Index");
            }

            var imagenUrl = ObtenerImagenUrl(nombre);
            var mazo = new MazoPokemon(nombre, 25.99m, imagenUrl);

            return View(mazo);
        }
 

    }
}
