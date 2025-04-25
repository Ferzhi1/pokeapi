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

    public PokemonController(PokemonService pokemonService, CheckoutService checkoutService, PokemonStorageService pokemonStorageService, PokemonVentaService pokemonVentaService)
    {
        _pokemonService = pokemonService;
        _checkoutService = checkoutService;
        _pokemonStorageService = pokemonStorageService;
        _pokemonVentaService = pokemonVentaService;
    }

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

    
    [HttpPost]
    public IActionResult GuardarFavorito([FromBody] ProductoPokemon pokemon)
    {
        if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre) || string.IsNullOrWhiteSpace(pokemon.Email))
        {
            Console.WriteLine("❌ Error: Datos incompletos.");
            return BadRequest("Error: Datos del Pokémon incompletos.");
        }

        // 🔥 Asegurar que la descripción nunca sea NULL
        pokemon.Descripcion = string.IsNullOrWhiteSpace(pokemon.Descripcion) ? "Sin descripción" : pokemon.Descripcion;

        _pokemonStorageService.AgregarPokemonAFavoritos(pokemon.Email, pokemon);

        return Ok(new { mensaje = "✅ Pokémon guardado en colección.", stats = pokemon.Stats });
    }




    [HttpGet]
    public IActionResult Coleccion(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("❌ Error: Email no proporcionado.");
                return BadRequest("Debe especificar un email.");
            }

            Console.WriteLine($"🔍 Buscando colección de Pokémon para el usuario: {email}");

            var pokemonsGuardados = _pokemonStorageService.ObtenerColeccionPokemon(email);

            if (pokemonsGuardados == null || !pokemonsGuardados.Any())
            {
                Console.WriteLine("⚠️ No se encontraron Pokémon en la colección.");
                return View(new List<ProductoPokemon>());
            }

            Console.WriteLine($"✅ Enviando colección para {email}: {pokemonsGuardados.Count} Pokémon(s).");

            foreach (var pokemon in pokemonsGuardados)
            {
                if (pokemon.Stats == null || !pokemon.Stats.Any())
                {
                    Console.WriteLine($"🚨 Advertencia: {pokemon.Nombre} no tiene estadísticas definidas.");
                    pokemon.Stats = new List<StatPokemon>(); // ✅ Aseguramos que la lista no sea null
                }
                else
                {
                    Console.WriteLine($"📊 Estadísticas de {pokemon.Nombre}: {JsonConvert.SerializeObject(pokemon.Stats, Formatting.Indented)}");
                }
            }

            return View(pokemonsGuardados);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en Coleccion(): {ex.Message}");
            return StatusCode(500, "Error interno al recuperar la colección.");
        }
    }





}
