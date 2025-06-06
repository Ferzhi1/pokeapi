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
                    return BadRequest("Los datos de la solicitud no son válidos.");
                }

                var resultado = await _solicitudService.EnviarSolicitudAsync(solicitud.RemitenteEmail, solicitud.ReceptorEmail);
                if (!resultado) throw new Exception("La solicitud ya existe o no se pudo crear.");

                if (AmistadHub.UsuariosConectados.TryGetValue(solicitud.ReceptorEmail, out var connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("RecibirSolicitud", solicitud.RemitenteEmail);
                }

                return Ok(new { mensaje = "Solicitud enviada correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error al enviar la solicitud: {ex.Message}" });
            }
        }

        [HttpGet("ListaSolicitudes")]
        public async Task<IActionResult> ListaSolicitudes()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("ListaSolicitudes", new List<SolicitudAmistad>());
            }

            var usuarioEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                               ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(usuarioEmail))
            {
                return View("ListaSolicitudes", new List<SolicitudAmistad>());
            }

            var solicitudes = await _context.SolicitudAmistad
                .Where(sa => sa.ReceptorEmail == usuarioEmail && sa.Estado == EstadoSolicitud.Pendiente)
                .ToListAsync();

            ViewBag.EmailUsuario = usuarioEmail;

            return View("ListaSolicitudes", solicitudes);
        }

        [HttpGet("ObtenerSolicitudId")]
        public async Task<IActionResult> ObtenerSolicitudId(string remitenteEmail)
        {
            var solicitud = await _solicitudService.ObtenerSolicitudPendientePorEmailAsync(remitenteEmail);

            if (solicitud == null)
            {
                return NotFound("No se encontró la solicitud.");
            }

            return Ok(new { solicitudId = solicitud.Id });
        }



        [HttpPost("AceptarSolicitud")]
        public async Task<IActionResult> AceptarSolicitud([FromBody] Dictionary<string, int> data)
        {
            if (!data.TryGetValue("solicitudId", out int solicitudId) || solicitudId <= 0)
            {
                return BadRequest("ID de solicitud no válido.");
            }

            var resultado = await _solicitudService.AceptarSolicitudAsync(solicitudId);
            return resultado ? Ok("Solicitud aceptada.") : BadRequest("Error al aceptar solicitud.");
        }







    }
}
