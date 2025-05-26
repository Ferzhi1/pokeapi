using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace api3.Hubs
{
    public class AmistadHub : Hub
    {
        private static ConcurrentDictionary<string, string> UsuariosConectados = new ConcurrentDictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            var email = Context.User?.Identity?.IsAuthenticated == true
                        ? Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                        : null;

            if (!string.IsNullOrEmpty(email))
            {
                UsuariosConectados[email] = Context.ConnectionId;
                Console.WriteLine($"🟢 Usuario conectado: {email} ({Context.ConnectionId})");
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var usuarioDesconectado = UsuariosConectados.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(usuarioDesconectado.Key))
            {
                UsuariosConectados.TryRemove(usuarioDesconectado.Key, out _);
                Console.WriteLine($"🔴 Usuario desconectado: {usuarioDesconectado.Key}");
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task EnviarSolicitudAmistad(string remitenteEmail, string receptorEmail)
        {
            if (!UsuariosConectados.ContainsKey(receptorEmail))
            {
                Console.WriteLine($"⚠️ Usuario {receptorEmail} no está conectado.");
                return;
            }

            await Clients.Client(UsuariosConectados[receptorEmail]).SendAsync("RecibirSolicitud", remitenteEmail);
        }

        public async Task AceptarSolicitudAmistad(string receptorEmail, string remitenteEmail)
        {
            if (!UsuariosConectados.ContainsKey(remitenteEmail))
            {
                Console.WriteLine($"⚠️ Usuario {remitenteEmail} no está conectado.");
                return;
            }

            await Clients.Client(UsuariosConectados[remitenteEmail]).SendAsync("SolicitudAceptada", receptorEmail);
        }

        public async Task RechazarSolicitudAmistad(string receptorEmail, string remitenteEmail)
        {
            if (!UsuariosConectados.ContainsKey(remitenteEmail))
            {
                Console.WriteLine($"⚠️ Usuario {remitenteEmail} no está conectado.");
                return;
            }

            await Clients.Client(UsuariosConectados[remitenteEmail]).SendAsync("SolicitudRechazada", receptorEmail);
        }
    }
}
