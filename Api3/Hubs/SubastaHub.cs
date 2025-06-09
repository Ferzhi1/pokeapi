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
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotificarActualizarOferta(int pokemonId, string usuario, decimal monto)
        {
            await Clients.All.SendAsync("ActualizarOferta", pokemonId, usuario, monto);
        }

        public async Task NotificarNuevaSubasta(int pokemonId, string nombrePokemon, string rareza, decimal precioInicial, string imagenUrl, int duracionMinutos, string emailVendedor, decimal pujaActual, List<StatPokemon> stats)
        {
            await Clients.All.SendAsync("NuevaSubasta", pokemonId, nombrePokemon, rareza, precioInicial, imagenUrl, duracionMinutos, emailVendedor, pujaActual, stats);
        }


    }
}
