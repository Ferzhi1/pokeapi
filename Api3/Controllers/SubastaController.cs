using Microsoft.AspNetCore.Mvc;
using api3.Services;
using Microsoft.EntityFrameworkCore;
using api3.Models;
using Microsoft.AspNetCore.SignalR;

[Route("Subasta")]
public class SubastaController : Controller
{
    private readonly SubastaService _subastaService;

    private readonly ApplicationDbContext _context;

    public SubastaController(SubastaService subastaService, ApplicationDbContext context)
    {
        _subastaService = subastaService ?? throw new ArgumentNullException(nameof(subastaService));

        _context = context;
    }



    [HttpPost("PujarPokemon")]
    public async Task<IActionResult> PujarPokemon([FromBody] OfertaDto oferta)
    {
        var pokemon = await _context.ProductoPokemon.FirstOrDefaultAsync(p => p.Id == oferta.PokemonId);

        if (pokemon == null || oferta == null || oferta.PokemonId <= 0 || string.IsNullOrEmpty(oferta.Usuario) || oferta.Monto <= 0 || DateTime.Now >= pokemon.TiempoExpiracion)
        {
            return BadRequest("❌ Datos de oferta inválidos o la subasta ha finalizado.");
        }


        if (pokemon.Email == oferta.Usuario)
        {
            return BadRequest(new { error = "❌ No puedes pujar por tu propio Pokémon." });
        }

        var resultado = await _subastaService.RegistrarOfertaAsync(oferta.PokemonId, oferta.Usuario, oferta.Monto);
        if (!resultado)
        {
            return BadRequest("❌ La oferta debe ser mayor a la puja actual.");
        }


        return Ok(new { mensaje = "✅ Oferta realizada." });
    }

    [HttpPost("FinalizarSubasta")]
    public async Task<IActionResult> FinalizarSubasta([FromBody] Dictionary<string, int> data)
    {
        if (!data.TryGetValue("pokemonId", out int pokemonId) || pokemonId <= 0)
        {
            return BadRequest(new { error = "❌ ID de Pokémon no válido." });
        }

        var pokemon = await _context.ProductoPokemon
            .Include(p => p.HistorialPujas)
            .FirstOrDefaultAsync(p => p.Id == pokemonId);

        if (pokemon == null)
        {
            return NotFound(new { error = "❌ Pokémon no encontrado en subasta." });
        }

        await _subastaService.FinalizarSubastaAsync(pokemonId);

        var pujaGanadora = await _context.Puja
            .Where(p => p.PokemonId == pokemonId)
            .OrderByDescending(p => p.CantidadMonedas)
            .FirstOrDefaultAsync();

        if (pujaGanadora == null || pujaGanadora.UsuarioEmail == pokemon.Email)
        {
            pokemon.EnVenta = false;
            pokemon.Precio = 0;

            _context.ProductoPokemon.Update(pokemon);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"❌ Subasta del Pokémon {pokemon.Nombre} finalizada sin cambios.", sinPujas = true });

        }

        bool tieneDescuento = await _context.SolicitudAmistad
            .AnyAsync(s =>
                (s.RemitenteEmail == pujaGanadora.UsuarioEmail && s.ReceptorEmail == pokemon.Email) ||
                (s.ReceptorEmail == pujaGanadora.UsuarioEmail && s.RemitenteEmail == pokemon.Email) &&
                s.Estado == EstadoSolicitud.Aceptada);

        decimal precioFinal = tieneDescuento ? pujaGanadora.CantidadMonedas * 0.7m : pujaGanadora.CantidadMonedas;

        var comprador = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == pujaGanadora.UsuarioEmail);
        if (comprador != null)
        {
            comprador.Monedero -= precioFinal;
            _context.UsuariosPokemonApi.Update(comprador);
            await _context.SaveChangesAsync();
        }

        var pokemonColeccion = new ColeccionPokemon
        {
            Id = pokemon.Id,
            Nombre = pokemon.Nombre,
            ImagenUrl = pokemon.ImagenUrl,
            Rareza = pokemon.Rareza,
            EmailUsuario = pujaGanadora.UsuarioEmail
        };

        _context.ColeccionPokemon.Add(pokemonColeccion);
        _context.ProductoPokemon.Remove(pokemon);

        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"❌ Subasta del Pokémon {pokemon.Nombre} finalizada sin cambios.", sinPujas = true });
    }
}






















public class OfertaDto
{
    public int PokemonId { get; set; }
    public string Usuario { get; set; }
    public decimal Monto { get; set; }
}
