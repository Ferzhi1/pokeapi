using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using api3.Services;
using api3.Hubs;

[Route("Subasta")]
public class SubastaController : Controller
{
    private readonly SubastaService _subastaService;
    private readonly IHubContext<SubastaHub> _hubContext;

    public SubastaController(SubastaService subastaService, IHubContext<SubastaHub> hubContext)
    {
        _subastaService = subastaService ?? throw new ArgumentNullException(nameof(subastaService));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    [HttpPost("PujarPokemon")]
    public async Task<IActionResult> PujarPokemon([FromBody] OfertaDto oferta)
    {
        if (oferta == null || oferta.PokemonId <= 0 || string.IsNullOrEmpty(oferta.Usuario) || oferta.Monto <= 0)
        {
            return BadRequest("❌ Datos de oferta inválidos.");
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
}

public class OfertaDto
{
    public int PokemonId { get; set; }
    public string Usuario { get; set; }
    public decimal Monto { get; set; }
}
