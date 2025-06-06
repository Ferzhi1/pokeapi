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
        public async Task NotificarNuevaSubasta(int pokemonId, string nombrePokemon, decimal precioInicial)
        {
            await Clients.All.SendAsync("NuevaSubasta", pokemonId, nombrePokemon, precioInicial);
        }

        public async Task FinalizarSubasta(int pokemonId, string ganador)
        {
            await Clients.All.SendAsync("SubastaFinalizada", pokemonId, ganador);
            Console.WriteLine($"🏆 Subasta finalizada. Ganador: {ganador}, Pokémon: {pokemonId}");
        }

    }
}
