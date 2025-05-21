using api3.Models;
using api3.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace api3.Controllers
{
    public class VentaController : Controller
    {
        private readonly VentaService _ventaService;

        public VentaController(VentaService ventaService)
        {
            _ventaService = ventaService;
        }

        [HttpPost]
        public IActionResult GuardarVenta([FromBody] ProductoPokemon pokemon)
        {
            if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre) || string.IsNullOrWhiteSpace(pokemon.Email))
                return BadRequest("❌ Error: Datos del Pokémon incompletos.");

            pokemon.Descripcion ??= "Sin descripción";
            _ventaService.AgregarPokemonAVenta(pokemon.Email, pokemon);

            return Ok(new { mensaje = "✅ Pokémon guardado en colección.", stats = pokemon.Stats });
        }

        [HttpGet]
        public IActionResult Venta()
        {
            var emailUsuarioAutenticado = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var monederoUsuario = _ventaService.ObtenerSaldo(emailUsuarioAutenticado); // Suponiendo que este método devuelve el saldo

            if (string.IsNullOrWhiteSpace(emailUsuarioAutenticado))
            {
                Console.WriteLine("⚠️ No se pudo obtener el email del usuario autenticado.");
                return RedirectToAction("Login");
            }

            ViewBag.EmailUsuario = emailUsuarioAutenticado;
            ViewBag.Monedero = monederoUsuario;

            var pokemonsGuardados = _ventaService.ObtenerVentaPokemon(emailUsuarioAutenticado) ?? new List<ProductoPokemon>();

            foreach (var pokemon in pokemonsGuardados)
                pokemon.Stats ??= new List<StatPokemon>();

            return View(pokemonsGuardados);
        }


        [HttpPost]
        public IActionResult VenderCarta([FromBody] ProductoPokemon pokemon)
        {
            if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Email))
                return BadRequest("❌ Error: Datos incompletos.");

            bool ventaExitosa = _ventaService.VenderCarta(pokemon.Email, pokemon);

            if (!ventaExitosa)
                return BadRequest("📁 No se pudo procesar la venta.");
            var nuevoSaldo = _ventaService.ObtenerSaldo(pokemon.Email);
            Console.WriteLine($"🔄 Saldo actualizado para {pokemon.Email}: {nuevoSaldo} monedas");
            return Ok(new { mensaje = "✅ Venta realizada con éxito.", nuevoSaldo });

        }



    }
}
