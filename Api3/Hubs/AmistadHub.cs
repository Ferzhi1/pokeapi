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
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
             
                UsuariosConectados.AddOrUpdate(email, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);

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
            var email = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                UsuariosConectados.TryRemove(email, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task EnviarSolicitudAmistad(string remitenteEmail, string receptorEmail)
        {
            if (!UsuariosConectados.ContainsKey(receptorEmail)) return; 

            await Clients.User(receptorEmail).SendAsync("RecibirSolicitud", remitenteEmail);
        }

        public async Task AceptarSolicitudAmistad(string receptorEmail, string remitenteEmail)
        {
            if (!UsuariosConectados.ContainsKey(remitenteEmail)) return;

            await Clients.Client(UsuariosConectados[remitenteEmail]).SendAsync("SolicitudAceptada", receptorEmail);
        }
    }
}
