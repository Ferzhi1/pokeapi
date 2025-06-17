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
    public async Task<IActionResult> FinalizarSubasta([FromBody] OfertaDto oferta)
    {
        if (oferta == null || oferta.PokemonId <= 0 || string.IsNullOrWhiteSpace(oferta.Usuario) || oferta.Monto <= 0)
        {
            return BadRequest(new { error = "❌ Datos de oferta incompletos o inválidos." });
        }

        var resultado = await _subastaService.FinalizarSubastaAsync(oferta);

        if (resultado.SinPujas)
        {
            return Ok(new
            {
                mensaje = $"⚠️ Subasta del Pokémon {resultado.NombrePokemon} finalizada sin pujas.",
                sinPujas = true
            });
        }

        return Ok(new
        {
            mensaje = $"✅ Subasta del Pokémon {resultado.NombrePokemon} finalizada exitosamente.",
            sinPujas = false,
            ganador = resultado.Ganador
        });
    }


}
























public class OfertaDto
{
    public int PokemonId { get; set; }
    public string Usuario { get; set; }
    public decimal Monto { get; set; }
}
