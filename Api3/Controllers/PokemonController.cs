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
        Console.WriteLine($"📩 Datos recibidos - Stats: {JsonConvert.SerializeObject(pokemon.Stats, Formatting.Indented)}");

        if (pokemon.Stats == null || pokemon.Stats.Count == 0)
        {
            Console.WriteLine($"⚠ Estadísticas no recibidas correctamente para {pokemon.Nombre}. Se asigna lista vacía.");
            pokemon.Stats = new List<StatPokemon>();
        }


        _pokemonStorageService.AgregarPokemonAFavoritos("hugo@test.com", pokemon);
        
        return Ok(new { mensaje = "Pokémon guardado exitosamente", stats = pokemon.Stats });
    }


    [HttpGet]
    public IActionResult Coleccion()
    {
        var pokemonsGuardados = _pokemonStorageService.ObtenerColeccionPokemon("hugo@test.com");

        if (pokemonsGuardados == null || pokemonsGuardados.Count == 0)
        {
            Console.WriteLine("⚠️ No se encontraron Pokémon en la colección.");
            return Ok(new { mensaje = "No hay Pokémon guardados en la colección.", pokemons = new List<ProductoPokemon>() });
        }

        Console.WriteLine($"✅ Enviando colección al frontend: {JsonConvert.SerializeObject(pokemonsGuardados, Formatting.Indented)}");

        return View(pokemonsGuardados);
    }



    [HttpPost]
    public IActionResult GuardarParaVenta([FromBody] ProductoPokemon pokemon)
    {
        Console.WriteLine($"📩 Datos recibidos para venta - Stats: {JsonConvert.SerializeObject(pokemon.Stats, Formatting.Indented)}");

        if (pokemon.Stats == null || pokemon.Stats.Count == 0)
        {
            Console.WriteLine($"⚠ Estadísticas no recibidas correctamente para {pokemon.Nombre}. Se asigna lista vacía.");
            pokemon.Stats = new List<StatPokemon>();
        }

        _pokemonStorageService.AgregarPokemonAVenta("hugo@test.com", pokemon); // ✅ Guarda en la lista de venta, no en favoritos

        return Ok(new { mensaje = "Pokémon agregado para venta exitosamente", stats = pokemon.Stats });
    }


    [HttpGet]
    public IActionResult Venta()
    {
        var pokemonsEnVenta = _pokemonStorageService.ObtenerPokemonEnVenta("hugo@test.com");

        if (pokemonsEnVenta == null || pokemonsEnVenta.Count == 0)
        {
            Console.WriteLine("⚠️ No se encontraron Pokémon en venta.");
            return Ok(new { mensaje = "No hay Pokémon disponibles para vender.", pokemons = new List<ProductoPokemon>() });
        }

        Console.WriteLine($"✅ Enviando lista de Pokémon en venta al frontend: {JsonConvert.SerializeObject(pokemonsEnVenta, Formatting.Indented)}");

        return View(pokemonsEnVenta);
    }


}
