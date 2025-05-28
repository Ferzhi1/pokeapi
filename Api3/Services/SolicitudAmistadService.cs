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
            // 🔹 Verificar si ya existe una solicitud con este remitente y receptor
            var solicitudExistente = await _context.SolicitudAmistad
                .FirstOrDefaultAsync(s => s.RemitenteEmail == remitenteEmail && s.ReceptorEmail == receptorEmail && s.Estado == EstadoSolicitud.Pendiente);

            if (solicitudExistente != null)
            {
                Console.WriteLine("❌ La solicitud ya existe en la base de datos.");
                return false; // 🔹 Evita que el backend intente crear una solicitud duplicada
            }

            var nuevaSolicitud = new SolicitudAmistad
            {
                RemitenteEmail = remitenteEmail,
                ReceptorEmail = receptorEmail,
                FechaEnvio = DateTime.Now,
                Estado = EstadoSolicitud.Pendiente
            };

            _context.SolicitudAmistad.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<SolicitudAmistad>> ObtenerSolicitudesPendientesAsync(string usuarioEmail)
        {
            if (string.IsNullOrWhiteSpace(usuarioEmail))
            {
                Console.WriteLine("❌ Error: Email inválido.");
                return new List<SolicitudAmistad>(); // Retorna lista vacía si el email no es válido
            }

            try
            {
                Console.WriteLine($"🔍 Buscando solicitudes pendientes para {usuarioEmail}...");
                Console.WriteLine($"📌 Estado Pendiente en Enum: {(int)EstadoSolicitud.Pendiente}"); // Depuración

                var solicitudes = await _context.SolicitudAmistad
                    .Where(sa => sa.ReceptorEmail == usuarioEmail && sa.Estado == EstadoSolicitud.Pendiente)
                    .ToListAsync();

                Console.WriteLine(solicitudes.Any()
                    ? $"📊 Solicitudes encontradas: {solicitudes.Count}"
                    : "⚠ No se encontraron solicitudes pendientes.");

                return solicitudes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al obtener solicitudes pendientes: {ex.Message}");
                return new List<SolicitudAmistad>(); // Retorna lista vacía en caso de error
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

            if (solicitud == null || solicitud.Estado != EstadoSolicitud.Pendiente)
            {
                Console.WriteLine($"❌ Error: La solicitud con ID {solicitudId} no existe o ya ha sido procesada.");
                return false;
            }

            
            solicitud.Estado = EstadoSolicitud.Aceptada;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Solicitud {solicitudId} aceptada: {solicitud.RemitenteEmail} ↔ {solicitud.ReceptorEmail}");

                
                await _hubContext.Clients.User(solicitud.RemitenteEmail).SendAsync("SolicitudAceptada", solicitud.ReceptorEmail);
                Console.WriteLine($"📡 Notificación enviada a {solicitud.RemitenteEmail}");

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error al aceptar solicitud: {ex.Message}");
                return false;
            }
        }




    }
}
