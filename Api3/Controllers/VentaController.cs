using api3.Models;
using api3.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace api3.Controllers
{
    public class VentaController : Controller
    {
        private readonly VentaService _ventaService;
        private readonly ApplicationDbContext _context;
        private readonly PokemonStorageService _pokemonStorageService;
        private readonly ClimaService _climaService;

        public VentaController(VentaService ventaService,ApplicationDbContext context, PokemonStorageService pokeStorageService, ClimaService climaService)
        {
            _ventaService = ventaService;
            _context = context;
            _pokemonStorageService = pokeStorageService;
            _climaService = climaService;
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
            if (!User.Identity.IsAuthenticated)
                return Unauthorized("El usuario no está autenticado.");

            var emailUsuarioAutenticado = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(emailUsuarioAutenticado))
                return BadRequest("No se pudo obtener el email del usuario autenticado.");

            var usuarioPokemon = _pokemonStorageService.ObtenerUsuarioPokemon(emailUsuarioAutenticado);
            if (usuarioPokemon == null)
                return BadRequest("El usuario no existe en la base de datos.");

            if (!usuarioPokemon.CorreoValidado)
                return BadRequest("Debes validar tu correo electrónico antes de proceder con la subasta.");

            var pokemon = _context.ProductoPokemon.Find(pokemonId);
            if (pokemon == null)
                return BadRequest("No se encontró el Pokémon.");

            if (pokemon.Email != emailUsuarioAutenticado)
                return BadRequest("No puedes subastar un Pokémon que no es tuyo.");

            pokemon.PrecioInicial = precioInicial;
            pokemon.PujaActual = precioInicial;
            pokemon.TiempoExpiracion = DateTime.Now.AddMinutes(duracionMinutos);
            pokemon.EnVenta = true;

            _context.SaveChanges();
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

            Console.WriteLine($"✅ Clima recibido: {JsonSerializer.Serialize(climaResponse)}"); // 🔍 Depuración

            // Obtener el email desde los Claims
            var emailUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;

            ViewBag.EmailUsuario = emailUsuario; // Pasarlo a la vista

            var viewModel = new MercadoViewModel
            {
                Pokemons = pokemonsEnVenta,
                Clima = climaResponse,
                UsuarioEmail = emailUsuario // También en el modelo
            };

            return View(viewModel);
        }






        [HttpPost]
        public IActionResult FinalizarSubasta(int pokemonId)
        {
            var pokemon = _context.ProductoPokemon.Find(pokemonId);

            if (pokemon == null)
                return BadRequest("❌ Error: No se encontró el Pokémon.");

            if (pokemon.TiempoExpiracion > DateTime.Now)
                return BadRequest("❌ La subasta aún está activa.");

         
            pokemon.EnVenta = false;
            pokemon.PujaActual = 0;
            pokemon.PrecioInicial = 0;
            pokemon.TiempoExpiracion = DateTime.MinValue;

            _context.Entry(pokemon).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok(new { mensaje = "✅ Subasta finalizada correctamente." });
        }

    }
}
