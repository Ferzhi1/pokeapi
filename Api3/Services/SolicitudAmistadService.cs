using api3.Hubs;
using api3.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            if (remitenteEmail == receptorEmail)
            {
                Console.WriteLine("❌ Error: No puedes enviarte una solicitud de amistad a ti mismo.");
                return false;
            }

            // Validar formato de correo electrónico
            if (!EsEmailValido(receptorEmail))
            {
                Console.WriteLine("❌ Error: Formato de email inválido.");
                return false;
            }

            var solicitudExistente = await _context.SolicitudAmistad
                .FirstOrDefaultAsync(sa => (sa.RemitenteEmail == remitenteEmail && sa.ReceptorEmail == receptorEmail) ||
                                           (sa.RemitenteEmail == receptorEmail && sa.ReceptorEmail == remitenteEmail));

            if (solicitudExistente != null)
            {
                if (solicitudExistente.Estado == EstadoSolicitud.Aceptada)
                {
                    Console.WriteLine("⚠️ Error: Ya son amigos.");
                    return false;
                }

                Console.WriteLine("⚠️ Solicitud ya existe en la base de datos.");
                return false;
            }

            var solicitud = new SolicitudAmistad
            {
                RemitenteEmail = remitenteEmail,
                ReceptorEmail = receptorEmail,
                Estado = EstadoSolicitud.Pendiente,
                FechaEnvio = DateTime.UtcNow
            };

            try
            {
                _context.SolicitudAmistad.Add(solicitud);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Solicitud guardada en la base de datos: {remitenteEmail} → {receptorEmail}");

                await _hubContext.Clients.User(receptorEmail).SendAsync("RecibirSolicitud", remitenteEmail);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error al guardar solicitud: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AceptarSolicitudAsync(int solicitudId)
        {
            var solicitud = await _context.SolicitudAmistad.FindAsync(solicitudId);
            if (solicitud == null || solicitud.Estado != EstadoSolicitud.Pendiente)
            {
                Console.WriteLine("⚠️ La solicitud no existe o ya fue procesada.");
                return false;
            }

            solicitud.Estado = EstadoSolicitud.Aceptada;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Solicitud aceptada: {solicitud.RemitenteEmail} → {solicitud.ReceptorEmail}");

                await _hubContext.Clients.User(solicitud.RemitenteEmail).SendAsync("SolicitudAceptada", solicitud.ReceptorEmail);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error al aceptar solicitud: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RechazarSolicitudAsync(int solicitudId)
        {
            var solicitud = await _context.SolicitudAmistad.FindAsync(solicitudId);
            if (solicitud == null || solicitud.Estado != EstadoSolicitud.Pendiente)
            {
                Console.WriteLine("⚠️ La solicitud no existe o ya fue procesada.");
                return false;
            }

            solicitud.Estado = EstadoSolicitud.Rechazada;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"❌ Solicitud rechazada: {solicitud.RemitenteEmail} → {solicitud.ReceptorEmail}");

                await _hubContext.Clients.User(solicitud.RemitenteEmail).SendAsync("SolicitudRechazada", solicitud.ReceptorEmail);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error al rechazar solicitud: {ex.Message}");
                return false;
            }
        }

        public async Task<List<SolicitudAmistad>> ObtenerSolicitudesPendientesAsync(string usuarioEmail)
        {
            Console.WriteLine($"🔍 Buscando solicitudes pendientes para {usuarioEmail}...");
            return await _context.SolicitudAmistad
                .Where(sa => sa.ReceptorEmail == usuarioEmail && sa.Estado == EstadoSolicitud.Pendiente)
                .ToListAsync();
        }

        public async Task<SolicitudAmistad?> ObtenerSolicitudPorIdAsync(int solicitudId)
        {
            return await _context.SolicitudAmistad.FindAsync(solicitudId);
        }

        // Método para validar formato de correo electrónico
        private bool EsEmailValido(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
    }
}
