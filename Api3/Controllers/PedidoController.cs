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

            // 🟢 Verificar si el usuario existe en la base de datos
            Console.WriteLine($"🔍 Buscando usuario con email: {email}");
            var usuarioPokemon = _pokemonStorageService.ObtenerUsuarioPokemon(email);

            if (usuarioPokemon == null)
            {
                Console.WriteLine("❌ ERROR: Usuario no encontrado en la base de datos.");
                return BadRequest("El usuario no existe en la base de datos.");
            }
            Console.WriteLine($"✅ Usuario encontrado: {usuarioPokemon.Email}");

            // 🔹 Definir cantidad de Pokémon según el mazo
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
            Console.WriteLine($"🔍 Buscando pedido existente para el usuario: {email}");
            var pedidoUsuario = _pokemonStorageService.ObtenerPedidoUsuario(email); // ✅ Corrección aquí

            if (pedidoUsuario == null || pedidoUsuario.MazoSeleccionado != nombreMazo)
            {
                Console.WriteLine("🆕 No hay pedido previo o el usuario cambió de mazo, generando nuevo.");
                var nuevosPokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

                var nuevoPedidoPokemon = new PedidoPokemon(nombreMazo, mazo.Precio, email)
                {
                    Pokemons = nuevosPokemons
                };

                // ✅ Asegurar que `EmailUsuario` NO sea nulo antes de guardar
                foreach (var pokemon in nuevoPedidoPokemon.Pokemons)
                {
                    pokemon.Email = email; // ✅ Corrección aquí
                }

                _pokemonStorageService.GuardarPedidoUsuario(email, nombreMazo, new List<PedidoPokemon> { nuevoPedidoPokemon });
                mazo.Pokemons = nuevosPokemons;
            }
            else
            {
                Console.WriteLine($"✅ Pedido existente encontrado: {pedidoUsuario.MazoSeleccionado}");
                mazo.Pokemons = pedidoUsuario.Pokemons.Select(p => new ProductoPokemon
                {
                    Nombre = p.NombreMazo,
                    Precio = p.Precio,
                    ImagenUrl = imagenUrl,
                    Email = email // ✅ Corrección aquí
                }).ToList();
            }

            var pedidoPokemon = new PedidoPokemon(nombreMazo, mazo.Precio, email)
            {
                Pokemons = mazo.Pokemons
            };

            Console.WriteLine("✔ Pedido listo, mostrando confirmación.");
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
