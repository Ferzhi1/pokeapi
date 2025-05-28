using api3.Hubs;
using api3.Models;
using api3.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api3.Controllers
{
    [Route("SolicitudAmistad")]
    public class SolicitudAmistadController : Controller
    {
        private readonly SolicitudAmistadService _solicitudService;
        private readonly IHubContext<AmistadHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public SolicitudAmistadController(SolicitudAmistadService solicitudService, IHubContext<AmistadHub> hubContext, ApplicationDbContext context)
        {
            _solicitudService = solicitudService;
            _hubContext = hubContext;
            _context = context;
        }


        [HttpPost("EnviarSolicitud")]
        public async Task<IActionResult> EnviarSolicitud([FromForm] SolicitudAmistad solicitud)
        {
            try
            {
                if (solicitud == null || string.IsNullOrEmpty(solicitud.RemitenteEmail) || string.IsNullOrEmpty(solicitud.ReceptorEmail))
                {
                    Console.WriteLine("❌ Error: Datos inválidos.");
                    return BadRequest("❌ Los datos de la solicitud no son válidos.");
                }

                Console.WriteLine($"📡 Intentando enviar solicitud de {solicitud.RemitenteEmail} a {solicitud.ReceptorEmail}");

                var resultado = await _solicitudService.EnviarSolicitudAsync(solicitud.RemitenteEmail, solicitud.ReceptorEmail);
                if (!resultado) throw new Exception("❌ La solicitud ya existe o no se pudo crear.");

                Console.WriteLine("✅ Solicitud enviada correctamente.");

                return Ok(new { mensaje = "✅ Solicitud enviada correctamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error crítico en EnviarSolicitud: {ex.Message}");
                return BadRequest(new { error = $"❌ Error al enviar la solicitud: {ex.Message}" });
            }
        }





        [HttpGet("ListaSolicitudes")]
        public async Task<IActionResult> ListaSolicitudes()
        {
            if (!User.Identity.IsAuthenticated)
            {
                Console.WriteLine("❌ Error: Usuario no autenticado.");
                TempData["Error"] = "❌ No estás autenticado.";
                return View("ListaSolicitudes", new List<SolicitudAmistad>());
            }

            var usuarioEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                               ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(usuarioEmail))
            {
                Console.WriteLine("❌ Error: Email obtenido desde Claims es inválido.");
                TempData["Error"] = "❌ No se pudo obtener tu email.";
                return View("ListaSolicitudes", new List<SolicitudAmistad>());
            }

            Console.WriteLine($"🔍 Buscando solicitudes pendientes para {usuarioEmail}...");
            Console.WriteLine($"📌 Estado Pendiente en Enum: {(int)EstadoSolicitud.Pendiente}");

            var solicitudes = await _context.SolicitudAmistad
                .Where(sa => sa.ReceptorEmail == usuarioEmail && sa.Estado == EstadoSolicitud.Pendiente)
                .ToListAsync();

            Console.WriteLine(solicitudes.Any()
                ? $"📊 Solicitudes encontradas: {solicitudes.Count}"
                : "⚠ No se encontraron solicitudes pendientes.");

            ViewBag.EmailUsuario = usuarioEmail;

            if (!solicitudes.Any())
            {
                TempData["Error"] = "❌ No tienes solicitudes pendientes.";
                return View("ListaSolicitudes", new List<SolicitudAmistad>());
            }

            return View("ListaSolicitudes", solicitudes);
        }
        [HttpGet("ObtenerSolicitudId")]
        public async Task<IActionResult> ObtenerSolicitudId(string remitenteEmail)
        {
            var solicitud = await _solicitudService.ObtenerSolicitudPendientePorEmailAsync(remitenteEmail);

            if (solicitud == null)
            {
                Console.WriteLine("❌ No se encontró la solicitud para " + remitenteEmail);
                return NotFound("❌ No se encontró la solicitud.");
            }

            Console.WriteLine($"📡 ID de solicitud encontrado: {solicitud.Id}");
            return Ok(new { solicitudId = solicitud.Id });
        }


        [HttpPost("AceptarSolicitud")]
        public async Task<IActionResult> AceptarSolicitud([FromBody] Dictionary<string, int> data)
        {
            if (!data.TryGetValue("solicitudId", out int solicitudId) || solicitudId <= 0)
            {
                Console.WriteLine("❌ ID de solicitud no válido recibido en el backend.");
                return BadRequest("❌ ID de solicitud no válido.");
            }

            Console.WriteLine($"📡 Solicitud recibida con ID válido: {solicitudId}");

            var resultado = await _solicitudService.AceptarSolicitudAsync(solicitudId);
            return resultado ? Ok("✅ Solicitud aceptada.") : BadRequest("❌ Error al aceptar solicitud.");
        }







    }
}
