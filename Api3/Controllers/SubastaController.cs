using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using api3.Services;
using api3.Hubs;
using Microsoft.EntityFrameworkCore;

[Route("Subasta")]
public class SubastaController : Controller
{
    private readonly SubastaService _subastaService;
    private readonly IHubContext<SubastaHub> _hubContext;
    private readonly ApplicationDbContext _context;

    public SubastaController(SubastaService subastaService, IHubContext<SubastaHub> hubContext, ApplicationDbContext context)
    {
        _subastaService = subastaService ?? throw new ArgumentNullException(nameof(subastaService));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _context = context;
    }

    [HttpPost("PujarPokemon")]
    public async Task<IActionResult> PujarPokemon([FromBody] OfertaDto oferta)
    {
        var pokemon = await _context.ProductoPokemon.FirstOrDefaultAsync(p => p.Id == oferta.PokemonId);

        if (oferta == null || oferta.PokemonId <= 0 || string.IsNullOrEmpty(oferta.Usuario) || oferta.Monto <= 0 || pokemon == null || DateTime.Now >= pokemon.TiempoExpiracion)
        {
            return BadRequest("❌ Datos de oferta inválidos o la subasta ha finalizado.");
        }




        var resultado = await _subastaService.RegistrarOfertaAsync(oferta.PokemonId, oferta.Usuario, oferta.Monto);
        if (!resultado)
        {
            return BadRequest("❌ La oferta debe ser mayor a la puja actual.");
        }
        Console.WriteLine($"Oferta recibida: Usuario: {oferta.Usuario}, Monto: {oferta.Monto}");



        await _hubContext.Clients.All.SendAsync("ActualizarOferta", oferta.PokemonId, oferta.Usuario, oferta.Monto);
        return Ok(new { mensaje = "✅ Oferta realizada." });

    }


    [HttpPost]
    public async Task<IActionResult> FinalizarSubasta(int pokemonId)
    {
        var resultado = await _subastaService.FinalizarSubastaAsync(pokemonId);

        if (!resultado)
        {
            return BadRequest("❌ No se pudo finalizar la subasta. Verifica que haya expirado.");
        }

        return Ok(new { mensaje = "✅ Subasta finalizada y el Pokémon ha sido transferido a la colección del ganador." });
    }









}

public class OfertaDto
{
    public int PokemonId { get; set; }
    public string Usuario { get; set; }
    public decimal Monto { get; set; }
}
