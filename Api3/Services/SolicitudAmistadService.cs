using api3.Hubs;
using api3.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api3.Services
{
    public class SolicitudAmistadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<AmistadHub> _hubContext;

        public SolicitudAmistadService(ApplicationDbContext context, IHubContext<AmistadHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<bool> EnviarSolicitudAsync(string remitenteEmail, string receptorEmail)
        {
            var solicitudExistente = await _context.SolicitudAmistad
                .FirstOrDefaultAsync(s => s.RemitenteEmail == remitenteEmail && s.ReceptorEmail == receptorEmail && s.Estado == EstadoSolicitud.Pendiente);

            if (solicitudExistente != null) return false;

            var nuevaSolicitud = new SolicitudAmistad
            {
                RemitenteEmail = remitenteEmail,
                ReceptorEmail = receptorEmail,
                FechaEnvio = DateTime.Now,
                Estado = EstadoSolicitud.Pendiente
            };

            _context.SolicitudAmistad.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            if (AmistadHub.UsuariosConectados.TryGetValue(receptorEmail, out var connectionId))
            {
                var solicitudesPendientes = await ObtenerSolicitudesPendientesAsync(receptorEmail);
                await _hubContext.Clients.Client(connectionId).SendAsync("ActualizarListaSolicitudes", solicitudesPendientes);
            }

            return true;
        }


        public async Task<List<SolicitudAmistad>> ObtenerSolicitudesPendientesAsync(string usuarioEmail)
        {
            if (string.IsNullOrWhiteSpace(usuarioEmail)) return new List<SolicitudAmistad>();

            try
            {
                return await _context.SolicitudAmistad
                    .Where(sa => sa.ReceptorEmail == usuarioEmail && sa.Estado == EstadoSolicitud.Pendiente)
                    .ToListAsync();
            }
            catch
            {
                return new List<SolicitudAmistad>();
            }
        }

        public async Task<SolicitudAmistad?> ObtenerSolicitudPendientePorEmailAsync(string remitenteEmail)
        {
            return await _context.SolicitudAmistad
                .FirstOrDefaultAsync(s => s.RemitenteEmail == remitenteEmail && s.Estado == EstadoSolicitud.Pendiente);
        }

        public async Task<bool> AceptarSolicitudAsync(int solicitudId)
        {
            var solicitud = await _context.SolicitudAmistad.FindAsync(solicitudId);

            if (solicitud == null || solicitud.Estado != EstadoSolicitud.Pendiente) return false;

            solicitud.Estado = EstadoSolicitud.Aceptada;

            try
            {
                await _context.SaveChangesAsync();
                await _hubContext.Clients.User(solicitud.RemitenteEmail).SendAsync("SolicitudAceptada", solicitud.ReceptorEmail);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
