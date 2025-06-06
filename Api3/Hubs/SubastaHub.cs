using api3.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace api3.Hubs
{
    public class SubastaHub : Hub
    {
        private static ConcurrentDictionary<string, string> UsuariosSubasta = new ConcurrentDictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var usuario = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(usuario))
            {
                UsuariosSubasta.TryAdd(usuario, Context.ConnectionId);
                Console.WriteLine($"✅ Usuario conectado: {usuario}, ID: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"❌ Usuario desconectado: {Context.ConnectionId}. Error: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotificarNuevaSubasta(int pokemonId, string nombrePokemon, decimal precioInicial, string imagenUrl, int duracionMinutos, string emailVendedor, decimal pujaActual)
        {
            await Clients.All.SendAsync("NuevaSubasta", pokemonId, nombrePokemon, precioInicial, imagenUrl, duracionMinutos, emailVendedor, pujaActual);
        }

        public async Task NotificarInicioSubasta(int pokemonId, string nombrePokemon, decimal precioInicial, int duracionMinutos, string imagenUrl)
        {
            await Clients.All.SendAsync("SubastaIniciada", pokemonId, nombrePokemon, precioInicial, duracionMinutos, imagenUrl);
        }

    }
}
