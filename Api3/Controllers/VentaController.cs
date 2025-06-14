﻿using api3.Models;
using api3.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using api3.Hubs;

namespace api3.Controllers
{
    public class VentaController : Controller
    {
        private readonly VentaService _ventaService;
        private readonly ApplicationDbContext _context;
        private readonly PokemonStorageService _pokemonStorageService;
        private readonly ClimaService _climaService;
        private readonly IHubContext<SubastaHub> _hubContext;

        public VentaController(VentaService ventaService,ApplicationDbContext context, PokemonStorageService pokeStorageService, ClimaService climaService,IHubContext<SubastaHub>hubContext)
        {
            _ventaService = ventaService;
            _context = context;
            _pokemonStorageService = pokeStorageService;
            _climaService = climaService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public IActionResult GuardarVenta([FromBody] ProductoPokemon pokemon)
        {
            if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre) || string.IsNullOrWhiteSpace(pokemon.Email))
                return BadRequest("Error: Datos del Pokémon incompletos.");

            pokemon.Descripcion ??= "Sin descripción";
            pokemon.EnVenta = true;

            _context.ProductoPokemon.Add(pokemon);
            _context.SaveChanges();

            return Ok(new { mensaje = "✅ Pokémon guardado para venta.", stats = pokemon.Stats });
        }


        [HttpGet]
        public IActionResult Venta()
        {
            var emailUsuarioAutenticado = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(emailUsuarioAutenticado))
            {
                return BadRequest("❌ Error: El email del usuario no está definido.");
            }

        
            var usuario = _context.UsuariosPokemonApi.FirstOrDefault(u => u.Email == emailUsuarioAutenticado);
            if (usuario == null)
            {
                return BadRequest("❌ Error: Usuario no encontrado.");
            }

            ViewBag.EmailUsuario = usuario.Email;
            ViewBag.Monedero = usuario.Monedero;

            var pokemonsEnVenta = _context.ProductoPokemon
                .Include(p => p.Stats) 
                .Where(p => p.Email == emailUsuarioAutenticado && p.EnVenta) 
                .ToList();

            return View(pokemonsEnVenta);
        }



        [HttpPost]
        public IActionResult IniciarSubasta(int pokemonId, decimal precioInicial, int duracionMinutos)
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();

            var emailUsuario = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(emailUsuario)) return BadRequest();

            var usuarioPokemon = _pokemonStorageService.ObtenerUsuarioPokemon(emailUsuario);
            var pokemon = _context.ProductoPokemon.Include(p => p.Stats).FirstOrDefault(p => p.Id == pokemonId);

            if (usuarioPokemon == null || !usuarioPokemon.CorreoValidado || pokemon == null || pokemon.Email != emailUsuario)
                return BadRequest();

            pokemon.PrecioInicial = precioInicial;
            pokemon.PujaActual = precioInicial;
            pokemon.TiempoExpiracion = DateTime.Now.AddMinutes(duracionMinutos);
            pokemon.EnVenta = true;

            _context.SaveChanges();

            _hubContext.Clients.All.SendAsync(
                "NuevaSubasta",
                pokemonId,
                pokemon.Nombre,
                pokemon.Rareza,
                precioInicial,
                pokemon.ImagenUrl,
                duracionMinutos,
                pokemon.Email,
                pokemon.PujaActual,
                pokemon.Stats,
                pokemon.TiempoExpiracion.Subtract(DateTime.Now).TotalMinutes

            );

            return RedirectToAction("Mercado");
        }





        public async Task<IActionResult> Mercado()
        {
            var pokemonsEnVenta = _context.ProductoPokemon
                .Include(p => p.Stats)
                .Where(p => p.EnVenta && p.TiempoExpiracion > DateTime.Now)
                .OrderBy(p => p.TiempoExpiracion)
                .ToList();

            var climaResponse = await _climaService.ObtenerClimaAsync("Bogotá");



           
            var emailUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;

            var usuario = _context.UsuariosPokemonApi.FirstOrDefault(u => u.Email == emailUsuario);


            ViewBag.Monedero = usuario?.Monedero ?? 0;
            ViewBag.EmailUsuario = emailUsuario;

            var viewModel = new MercadoViewModel
            {
                Pokemons = pokemonsEnVenta,
                Clima = climaResponse,
                UsuarioEmail = emailUsuario
            };

            return View(viewModel);
        }

    }
}
