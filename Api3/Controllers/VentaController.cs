using api3.Models;
using api3.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api3.Controllers
{
    public class VentaController : Controller
    {
        private readonly VentaService _ventaService;
        private readonly ApplicationDbContext _context;
        private readonly PokemonStorageService _pokemonStorageService;

        public VentaController(VentaService ventaService, ApplicationDbContext context, PokemonStorageService pokeStorageService)
        {
            _ventaService = ventaService;
            _context = context;
            _pokemonStorageService = pokeStorageService;
        }

        [HttpPost]
        public IActionResult GuardarVenta([FromBody] ProductoPokemon pokemon)
        {
            if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre) || string.IsNullOrWhiteSpace(pokemon.Email))
                return BadRequest("Error: Datos del Pokémon incompletos.");

            pokemon.Descripcion ??= "Sin descripción";
            _ventaService.AgregarPokemonAVenta(pokemon.Email, pokemon);

            return Ok(new { mensaje = "✅ Pokémon guardado en venta.", stats = pokemon.Stats });
        }

        [HttpGet]
        public IActionResult Venta()
        {
            var emailUsuario = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(emailUsuario))
                return BadRequest("❌ Error: El email del usuario no está definido.");

            ViewBag.EmailUsuario = emailUsuario;
            var pokemonsEnVenta = _ventaService.ObtenerVentaPokemon(emailUsuario);

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

        public IActionResult Mercado()
        {
            var pokemonsEnVenta = _context.ProductoPokemon
                .Include(p => p.Stats)
                .Where(p => p.EnVenta && p.TiempoExpiracion > DateTime.Now)
                .OrderBy(p => p.TiempoExpiracion)
                .ToList();

            return View(pokemonsEnVenta);
        }

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

            return RedirectToAction("Venta");
        }
    }
}
