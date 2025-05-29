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
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var usuario = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(usuario))
            {
                UsuariosSubasta.TryRemove(usuario, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotificarOferta(int pokemonId, string usuario, decimal monto)
        {
            await Clients.All.SendAsync("ActualizarOferta", pokemonId, usuario, monto);
        }
    }
}
