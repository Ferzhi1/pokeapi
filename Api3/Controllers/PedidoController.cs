﻿using System;
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
        private readonly IMemoryCache _cache;
        private ApplicationDbContext _context;

        public PedidoController(PokemonService pokemonService, CheckoutService checkoutService, PokemonStorageService pokemonStorageService, IMemoryCache cache, ApplicationDbContext context)
        {
            _pokemonService = pokemonService;
            _checkoutService = checkoutService;
            _pokemonStorageService = pokemonStorageService;
            _cache = cache;
            _context = context;
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
            if (!User.Identity.IsAuthenticated)
                return Unauthorized("El usuario no está autenticado.");

            var emailUsuarioAutenticado = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(emailUsuarioAutenticado))
                return BadRequest("No se pudo obtener el email del usuario autenticado.");

            var usuarioPokemon = _pokemonStorageService.ObtenerUsuarioPokemon(emailUsuarioAutenticado);

            if (usuarioPokemon == null)
                return BadRequest("El usuario no existe en la base de datos.");

            if (!usuarioPokemon.CorreoValidado)
                return BadRequest("Debes validar tu correo electrónico antes de proceder con la compra.");

            if (string.IsNullOrWhiteSpace(nombreMazo))
                return BadRequest("El nombre del mazo es obligatorio.");

            var cantidadPokemons = nombreMazo.Trim().ToLower() switch
            {
                "mazo pequeño" => 30,
                "mazo mediano" => 40,
                "mazo grande" => 50,
                _ => 30
            };

            string cacheKey = $"PokemonMazo_{cantidadPokemons}";
            if (!_cache.TryGetValue(cacheKey, out List<ProductoPokemon> pokemons))
            {
                pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

                if (pokemons == null || !pokemons.Any())
                    return BadRequest("No se encontraron Pokémon para este mazo.");

                _cache.Set(cacheKey, pokemons, TimeSpan.FromMinutes(30));
            }

            var pedidoPokemon = new PedidoPokemon(nombreMazo, 25.99m, emailUsuarioAutenticado)
            {
                Pokemons = pokemons
            };
            usuarioPokemon.Monedero += 1000;

            
            var usuarioExistente = _context.UsuariosPokemonApi.FirstOrDefault(u => u.Email == usuarioPokemon.Email);

            if (usuarioExistente != null)
            {
              
                usuarioExistente.Monedero = usuarioPokemon.Monedero;

             
                _context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("El usuario no existe en la base de datos.");
            }

            return View("~/Views/Pedido/Confirmacion.cshtml", pedidoPokemon);
        }

        [HttpGet("Checkout")]
        public async Task<IActionResult> Checkout(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return RedirectToAction("Index");

            var imagenUrl = ObtenerImagenUrl(nombre);
            if (string.IsNullOrWhiteSpace(imagenUrl))
                return BadRequest("No se encontró una imagen para el mazo.");

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

            string cacheKey = $"PokemonMazo_{cantidadPokemons}";

            if (!_cache.TryGetValue(cacheKey, out List<ProductoPokemon> pokemons))
            {
                pokemons = await _pokemonService.ObtenerPokemonsAsync(cantidadPokemons);

                if (pokemons == null || !pokemons.Any())
                    return BadRequest("No se encontraron Pokémon para este mazo.");

                _cache.Set(cacheKey, pokemons, TimeSpan.FromMinutes(30));
            }

            var mazo = new MazoPokemon(nombre, precioFijo, imagenUrl)
            {
                Pokemons = pokemons
            };

            return View(mazo);
        }
    }
}
