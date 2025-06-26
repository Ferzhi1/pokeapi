using api3.Models;
using api3.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Api3.Models;
using Microsoft.EntityFrameworkCore;
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
        if (pokemon == null || string.IsNullOrWhiteSpace(pokemon.Nombre) || string.IsNullOrWhiteSpace(pokemon.Email))
            return BadRequest("Error: Datos del Pokémon incompletos.");




        Console.WriteLine($"Recibiendo Pokémon favorito: {pokemon.Nombre} - {pokemon.Email}");

        
        var ultimoNumero = _context.ColeccionPokemon
            .Where(p => p.EmailUsuario == pokemon.Email)
            .Max(p => (int?)p.PokemonIdOriginal) ?? 0;

        var coleccionPokemon = new ColeccionPokemon
        {
            Nombre = pokemon.Nombre,
            ImagenUrl = pokemon.ImagenUrl,
            Rareza = pokemon.Rareza,
            EmailUsuario = pokemon.Email,
            Stats = pokemon.Stats,
            PokemonIdOriginal = pokemon.PokemonIdOriginal
        };

        _context.ColeccionPokemon.Add(coleccionPokemon);
        _context.SaveChanges();

       

        return Ok(new
        {
            mensaje = "✅ Pokémon guardado en la colección.",
            NumeroAlbum = coleccionPokemon.PokemonIdOriginal,
            stats = coleccionPokemon.Stats
        });
    }




    [HttpGet]
    public IActionResult Coleccion(int page = 1)
    {
        const int totalSlots = 1000;
        const int pageSize = 40;

        var emailUsuario = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(emailUsuario))
        {
            return RedirectToAction("Login");
        }

        var capturados = _context.ColeccionPokemon
            .Include(p => p.Stats)
            .Where(p => p.EmailUsuario == emailUsuario)
            .ToList();

   
        var listaCompleta = Enumerable.Range(1, totalSlots)
            .Select(id =>
                capturados.FirstOrDefault(p => p.PokemonIdOriginal == id)
                ?? new ColeccionPokemon { PokemonIdOriginal = id }
            ).ToList();

   
        var paginaActual = listaCompleta
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalSlots / pageSize);
        ViewBag.EmailUsuario = emailUsuario;
        var usuario = _context.UsuariosPokemonApi
        .AsNoTracking()
        .FirstOrDefault(u => u.Email == emailUsuario);

        ViewBag.Monedero = (int)(usuario?.Monedero ?? 0m);



        return View("Coleccion", paginaActual);
    }




}




