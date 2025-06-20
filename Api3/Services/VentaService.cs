using System.Collections.Concurrent;
using api3.Hubs;
using api3.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api3.Services
{
    public class VentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SubastaHub> _hubContext;
        private static ConcurrentDictionary<int, (Timer, int)> SubastasActivas = new();
        private readonly SubastaService _subastaService;

        public VentaService(ApplicationDbContext context, IHubContext<SubastaHub> hubContext, SubastaService service)
        {
            _context = context;
            _hubContext = hubContext;
            _subastaService = service;
        }

        public void AgregarPokemonAVenta(string email, ProductoPokemon pokemon)
        {
            pokemon.Descripcion ??= "Sin descripción";
            _context.ProductoPokemon.Add(pokemon);
            _context.SaveChanges();
        }

        public List<ProductoPokemon> ObtenerVentaPokemon(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new List<ProductoPokemon>();

            return _context.ProductoPokemon
                .Include(p => p.Stats)
                .Where(p => p.Email == email)
                .ToList();
        }
        public async Task<bool> IniciarSubastaAsync(int pokemonId, decimal precioInicial, int duracionMinutos, string usuarioEmail)
        {
            var pokemon = await _context.ProductoPokemon
                .Include(p => p.Stats)
                .FirstOrDefaultAsync(p => p.Id == pokemonId && p.Email == usuarioEmail);

            if (pokemon == null) return false;

            pokemon.UltimoDueno = usuarioEmail;
            pokemon.PrecioInicial = precioInicial;
            pokemon.PujaActual = precioInicial;
            pokemon.TiempoExpiracion = DateTime.Now.AddMinutes(duracionMinutos);
            pokemon.EnVenta = true;
            pokemon.fechaInicioSubasta = DateTime.Now;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("NuevaSubasta",
                pokemon.Id,
                pokemon.Nombre,
                pokemon.Rareza,
                precioInicial,
                pokemon.ImagenUrl,
                duracionMinutos,
                pokemon.Email,
                pokemon.PujaActual,
                pokemon.Stats,
                pokemon.TiempoExpiracion.Subtract(DateTime.Now).TotalMinutes
            );

            _subastaService.IniciarTemporizador(pokemon.Id, pokemon.Email, duracionMinutos);

            return true;
        }



    }
}
