using api3.Models;
using api3.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
[Authorize]
public class PokemonController : Controller
{
    private readonly PokemonService _pokemonService;
    private readonly CheckoutService _checkoutService;
    private readonly PokemonStorageService _pokemonStorageService;
    private readonly ApplicationDbContext _context;


    public PokemonController(PokemonService pokemonService, CheckoutService checkoutService, PokemonStorageService pokemonStorageService, ApplicationDbContext context)
    {
        _pokemonService = pokemonService;
        _checkoutService = checkoutService;
        _pokemonStorageService = pokemonStorageService;
        _context = context; 
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

        Console.WriteLine($"Recibiendo Pokémon: {pokemon.Nombre} - {pokemon.Email}"); 

        pokemon.Descripcion ??= "Sin descripción";
        pokemon.EnVenta = false;

        _context.ProductoPokemon.Add(pokemon);
        _context.SaveChanges();

        Console.WriteLine("✅ Pokémon guardado en la colección"); 

        return Ok(new { mensaje = "✅ Pokémon guardado en la colección.", stats = pokemon.Stats });
    }



    [HttpGet]
    public IActionResult Coleccion()
    {
        var emailUsuarioAutenticado = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrWhiteSpace(emailUsuarioAutenticado))
        {
            return RedirectToAction("Login");
        }

        var pokemonsGuardados = _context.ProductoPokemon
            .Where(p => p.Email == emailUsuarioAutenticado && !p.EnVenta) 
            .ToList();

        if (pokemonsGuardados == null || !pokemonsGuardados.Any())
        {
            return View(new List<ProductoPokemon>());
        }

        foreach (var pokemon in pokemonsGuardados)
        {
            pokemon.Stats ??= new List<StatPokemon>();
        }

        return View(pokemonsGuardados);
    }
}




