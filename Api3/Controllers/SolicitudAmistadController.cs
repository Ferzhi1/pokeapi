using System.Security.Claims;
using api3.Services;
using api3.Models;
using api3.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace api3.Controllers
{
    public class SolicitudAmistadController : Controller
    {
        private readonly SolicitudAmistadService _amistadService;
        private readonly IHubContext<AmistadHub> _hubContext;

        public SolicitudAmistadController(SolicitudAmistadService amistadService, IHubContext<AmistadHub> hubContext)
        {
            _amistadService = amistadService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> ListaSolicitudes()
        {
            var emailUsuario = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailUsuario))
                return Challenge();

            var solicitudesPendientes = await _amistadService.ObtenerSolicitudesPendientesAsync(emailUsuario);
            return View(solicitudesPendientes);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarSolicitud([FromBody] string receptorEmail)
        {
            var remitenteEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(remitenteEmail))
                return Challenge();

            receptorEmail = receptorEmail?.Trim(); // Eliminamos espacios innecesarios

            // Validar formato de correo electrónico
            if (!EsEmailValido(receptorEmail))
            {
                Console.WriteLine("❌ Error: Formato de email inválido.");
                return BadRequest("❌ El email del receptor no es válido.");
            }

            try
            {
                var resultado = await _amistadService.EnviarSolicitudAsync(remitenteEmail, receptorEmail);
                return resultado ? Ok("✅ Solicitud enviada correctamente.") : BadRequest("⚠️ No se pudo enviar la solicitud. Puede que ya exista.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error en EnviarSolicitud: {ex.Message}");
                return StatusCode(500, "❌ Error interno al procesar la solicitud.");
            }
        }

        [HttpPost("AceptarSolicitud/{id}")]
        public async Task<IActionResult> AceptarSolicitud(int id)
        {
            try
            {
                var resultado = await _amistadService.AceptarSolicitudAsync(id);
                if (!resultado) return BadRequest("⚠️ No se pudo aceptar la solicitud.");

                Console.WriteLine($"✅ Solicitud aceptada con ID {id}");
                return Ok("✅ Solicitud aceptada correctamente.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error en AceptarSolicitud: {ex.Message}");
                return StatusCode(500, "❌ Error interno al procesar la solicitud.");
            }
        }

        [HttpPost("RechazarSolicitud/{id}")]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            try
            {
                var resultado = await _amistadService.RechazarSolicitudAsync(id);
                if (!resultado) return BadRequest("⚠️ No se pudo rechazar la solicitud.");

                Console.WriteLine($"❌ Solicitud rechazada con ID {id}");
                return Ok("❌ Solicitud rechazada correctamente.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"❌ Error en RechazarSolicitud: {ex.Message}");
                return StatusCode(500, "❌ Error interno al procesar la solicitud.");
            }
        }

        // Método para validar formato de correo electrónico
        private bool EsEmailValido(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
    }
}
