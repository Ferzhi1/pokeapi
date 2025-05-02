using api3.Models;
using api3.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

public class PokemonController : Controller
{
    private readonly PokemonService _pokemonService;
    private readonly CheckoutService _checkoutService;
    private readonly PokemonStorageService _pokemonStorageService;
   

    public PokemonController(PokemonService pokemonService, CheckoutService checkoutService, PokemonStorageService pokemonStorageService)
    {
        _pokemonService = pokemonService;
        _checkoutService = checkoutService;
        _pokemonStorageService = pokemonStorageService;
      
    }

    public async Task<IActionResult> Index()
    {
        var mazos = new List<MazoPokemon>
        {
            new("Mazo Pequeño", 25.99m, "/img/mazo1.jpg"),
            new("Mazo Mediano", 39.99m, "/img/mazo2.jpg"),
            new("Mazo Grande", 69.99m, "/img/mazo6.jpg")
        };

        return View(mazos);
    }

    [HttpPost]
    public IActionResult GuardarFavorito([FromBody] ProductoPokemon pokemon)
    {
        if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre) || string.IsNullOrWhiteSpace(pokemon.Email))
            return BadRequest("Error: Datos del Pokémon incompletos.");

        pokemon.Descripcion ??= "Sin descripción";
        _pokemonStorageService.AgregarPokemonAFavoritos(pokemon.Email, pokemon);

        return Ok(new { mensaje = "✅ Pokémon guardado en colección.", stats = pokemon.Stats });
    }

    [HttpGet]
    public IActionResult Coleccion(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Debe especificar un email.");

        var pokemonsGuardados = _pokemonStorageService.ObtenerColeccionPokemon(email);
        if (pokemonsGuardados == null || !pokemonsGuardados.Any())
            return View(new List<ProductoPokemon>());

        foreach (var pokemon in pokemonsGuardados)
            pokemon.Stats ??= new List<StatPokemon>();

        return View(pokemonsGuardados);
    }
}
