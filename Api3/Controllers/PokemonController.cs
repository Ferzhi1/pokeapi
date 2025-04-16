using api3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using api3.Services;
using Newtonsoft.Json;

public class PokemonController : Controller
{
    private readonly PokemonService _pokemonService;
    private readonly CheckoutService _checkoutService;
    private readonly PokemonStorageService _pokemonStorageService;
        private readonly PokemonVentaService _pokemonVentaService;

        // 🔹 Constructor with dependency injection
        public PokemonController(PokemonService pokemonService, CheckoutService checkoutService, PokemonStorageService pokemonStorageService, PokemonVentaService pokemonVentaService)
    {
        _pokemonService = pokemonService;
        _checkoutService = checkoutService;
        _pokemonStorageService = pokemonStorageService;
        _pokemonVentaService = pokemonVentaService;
    }



    // 🔹 Show available mazos
    public async Task<IActionResult> Index()
    {
        var mazos = new List<MazoPokemon>
        {
            new MazoPokemon("Mazo Pequeño", 25.99m, "/img/mazo1.jpg"),
            new MazoPokemon("Mazo Mediano", 39.99m, "/img/mazo2.jpg"),
            new MazoPokemon("Mazo Grande", 69.99m, "/img/mazo3.jpg")
        };

        return View(mazos);
    }

    public IActionResult GuardarFavorito(string nombre, string imagenUrl, string rareza)
    {
        string emailUsuario = "hugo@test.com";

        Console.WriteLine($"🖼️ URL recibida: {imagenUrl}");
        Console.WriteLine($"🔎 Rareza recibida: {rareza}");

        var pokemon = new ProductoPokemon
        {
            Nombre = nombre,
            ImagenUrl = imagenUrl,
            Rareza =rareza
        };

        Console.WriteLine($"✅ Pokémon antes de guardar en favoritos: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");

        _pokemonStorageService.AgregarPokemonAFavoritos(emailUsuario, pokemon);
        return Ok(new { mensaje = "Pokémon guardado exitosamente" });
    }
    public IActionResult Coleccion()
    {
        string emailUsuario = "hugo@test.com"; // 📌 Asegúrate de obtener el email correctamente

        if (string.IsNullOrWhiteSpace(emailUsuario))
        {
            TempData["Error"] = "❌ No se encontró el email del usuario. Por favor, inicia sesión.";
            return RedirectToAction("PedidoConfirmado"); // 🔄 Redirigir si falta el email
        }

        var coleccion = _pokemonStorageService.ObtenerColeccionPokemon(emailUsuario);

        if (coleccion == null || !coleccion.Any())
        {
            TempData["Error"] = "❌ No tienes Pokémon en tu colección.";
        }

        return View(coleccion);
    }
    public IActionResult GuardarEnVenta(string nombre, string imagenUrl, string rareza)
    {
        string emailUsuario = "hugo@test.com";

        Console.WriteLine($"🖼️ URL recibida: {imagenUrl}");
        Console.WriteLine($"🔎 Rareza recibida: {rareza}");

        var pokemon = new ProductoPokemon
        {
            Nombre = nombre,
            ImagenUrl = imagenUrl,
            Rareza = string.IsNullOrWhiteSpace(rareza) ? "Desconocida" : rareza // 🔥 Aquí está la corrección
        };

        Console.WriteLine($"✅ Pokémon antes de enviar a venta: {pokemon.Nombre} - Rareza: {pokemon.Rareza}");

        _pokemonVentaService.AgregarPokemonAVenta(emailUsuario, pokemon);
        return Ok(new { mensaje = "Pokémon agregado a la venta exitosamente" });
    }

    public IActionResult Venta()
    {
        var pokemonsEnVenta = _pokemonVentaService.ObtenerPokemonsEnVenta();

        if (pokemonsEnVenta == null || !pokemonsEnVenta.Any())
        {
            TempData["Error"] = "❌ No hay Pokémon en venta.";
        }

        return View(pokemonsEnVenta);
    }
}























