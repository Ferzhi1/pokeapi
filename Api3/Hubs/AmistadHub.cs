using api3.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace api3.Hubs
{
    public class AmistadHub : Hub
    {
        public static ConcurrentDictionary<string, string> UsuariosConectados = new();
        private readonly SolicitudAmistadService _solicitudService;


        public AmistadHub(SolicitudAmistadService solicitudService)
        {
            _solicitudService = solicitudService;
        }

        public override async Task OnConnectedAsync()
        {
            var email = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                UsuariosConectados[email] = Context.ConnectionId;

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

            if (!string.IsNullOrEmpty(email))
            {
                UsuariosConectados.TryRemove(email, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task EnviarSolicitudAmistad(string remitenteEmail, string receptorEmail)
        {
            if (UsuariosConectados.TryGetValue(receptorEmail, out var connectionId))
            {
                await Clients.User(receptorEmail).SendAsync("RecibirSolicitud", remitenteEmail);
            }
        }


        public async Task AceptarSolicitudAmistad(string receptorEmail, string remitenteEmail)
        {
            if (UsuariosConectados.TryGetValue(remitenteEmail, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("SolicitudAceptada", receptorEmail);
            }
        }

   
    }
}
