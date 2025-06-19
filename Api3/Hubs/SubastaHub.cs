using api3.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace api3.Hubs
{
    public class SubastaHub : Hub
    {
        // Almacena los usuarios conectados y su ConnectionId
        public static ConcurrentDictionary<string, string> UsuariosSubasta = new();

        public override async Task OnConnectedAsync()
        {
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                UsuariosSubasta[email] = Context.ConnectionId;
                Console.WriteLine($"✅ Usuario conectado al hub de subasta: {email} - ConnectionId: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var email = UsuariosSubasta.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (!string.IsNullOrEmpty(email))
            {
                UsuariosSubasta.TryRemove(email, out _);
                Console.WriteLine($"🛑 Usuario desconectado del hub de subasta: {email}");
            }

            await base.OnDisconnectedAsync(exception);
        }


        public async Task NotificarActualizarOferta(int pokemonId, string usuario, decimal monto)
        {
            if (pokemonId > 0 && !string.IsNullOrEmpty(usuario) && monto > 0)
            {
                await Clients.All.SendAsync("ActualizarOferta", pokemonId, usuario, monto);
            }
        }


        public async Task NotificarNuevaSubasta(int pokemonId, string nombrePokemon, string rareza, decimal precioInicial, string imagenUrl, int duracionMinutos, string emailVendedor, decimal pujaActual, List<StatPokemon> stats)
        {
            if (pokemonId > 0 &&
                !string.IsNullOrEmpty(nombrePokemon) &&
                !string.IsNullOrEmpty(emailVendedor) &&
                precioInicial >= 0)
            {
                await Clients.All.SendAsync("NuevaSubasta", pokemonId, nombrePokemon, rareza, precioInicial, imagenUrl, duracionMinutos, emailVendedor, pujaActual, stats);
            }
        }

        public async Task ActualizarTiempoSubasta(int pokemonId, string emailVendedor, int tiempoRestante)
        {
            if (pokemonId > 0 && !string.IsNullOrEmpty(emailVendedor) && tiempoRestante >= 0)
            {
                await Clients.All.SendAsync("ActualizarTiempoSubasta", pokemonId, emailVendedor, tiempoRestante);
            }
        }

        public async Task ActualizarMonedero(string usuarioEmail, decimal nuevoSaldo)
        {
            await Clients.User(usuarioEmail).SendAsync("ActualizarMonedero", nuevoSaldo);
        }


        public async Task FinalizarSubasta(int pokemonId, string ganadorEmail, decimal montoFinal)
        {
            if (pokemonId > 0 && !string.IsNullOrEmpty(ganadorEmail) && montoFinal >= 0)
            {
                await Clients.All.SendAsync("SubastaFinalizada", pokemonId, ganadorEmail, montoFinal);
            }
        }

        public async Task NotificarEliminarCarta(int pokemonId)
        {
            if (pokemonId > 0)
            {
                await Clients.All.SendAsync("EliminarCarta", pokemonId);
            }
        }
    }
}
