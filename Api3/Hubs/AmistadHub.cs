using api3.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace api3.Hubs
{
    public class AmistadHub : Hub
    {
        private static ConcurrentDictionary<string, string> UsuariosConectados = new ConcurrentDictionary<string, string>();
        private readonly SolicitudAmistadService _solicitudService;

        public AmistadHub(SolicitudAmistadService solicitudService)
        {
            _solicitudService = solicitudService;
        }
        public override async Task OnConnectedAsync()
        {
            var email = Context.User?.Identity?.IsAuthenticated == true
                        ? Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                        : null;

            if (!string.IsNullOrEmpty(email))
            {
                UsuariosConectados[email] = Context.ConnectionId;
                Console.WriteLine($"🟢 Usuario conectado: {email} ({Context.ConnectionId})");

              
                var solicitudesPendientes = await _solicitudService.ObtenerSolicitudesPendientesAsync(email);
                foreach (var solicitud in solicitudesPendientes)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("RecibirSolicitud", solicitud.RemitenteEmail);
                }
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var email = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(email) && UsuariosConectados.ContainsKey(email))
            {
                UsuariosConectados.TryRemove(email, out _);
                Console.WriteLine($"🔴 Usuario desconectado: {email} ({Context.ConnectionId})");
            }

            // 🔹 Registrar el motivo de la desconexión
            Console.WriteLine($"⚠ Desconexión inesperada al enviar solicitud: {exception?.Message}");

            await base.OnDisconnectedAsync(exception);
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
            if (!UsuariosConectados.ContainsKey(remitenteEmail)) return;

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
