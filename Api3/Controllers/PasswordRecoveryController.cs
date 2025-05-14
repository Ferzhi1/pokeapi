using Microsoft.AspNetCore.Mvc;
using api3.Models;
using api3.Services;
using System.Threading.Tasks;

namespace api3.Controllers
{
    public class PasswordRecoveryController : Controller
    {
        private readonly PasswordRecoveryService _passwordRecoveryService;

        public PasswordRecoveryController(PasswordRecoveryService passwordRecoveryService)
        {
            _passwordRecoveryService = passwordRecoveryService;
        }

        [HttpGet]
        public IActionResult RequestReset()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestReset(ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _passwordRecoveryService.FindUserByEmailAsync(model.Email);
            if (user != null)
            {
                await _passwordRecoveryService.GenerateAndSetResetTokenAsync(user);
                // Aquí enviarías el correo electrónico con el enlace que contiene el token
                // Para este ejemplo simplificado, redirigimos directamente (INSEGURO PARA PRODUCCIÓN)
                return RedirectToAction("ResetPassword", new { token = user.ResetPasswordToken });
            }

            ViewBag.Message = "Si este correo electrónico existe en nuestra base de datos, se ha enviado un enlace para restablecer la contraseña.";
            return View("RequestResetConfirmation");
        }

        [HttpGet]
        public IActionResult RequestResetConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return View("Error"); // O una vista de error apropiada
            }
            return View(new ResetPasswordViewModel { ResetToken = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _passwordRecoveryService.FindUserByResetTokenAsync(model.ResetToken);
            if (user == null)
            {
                ModelState.AddModelError("", "El enlace de restablecimiento de contraseña no es válido o ha expirado.");
                return View(model);
            }

            // *** INSPECCIONA EL OBJETO 'user' AQUÍ EN EL DEBUGGER ***

            await _passwordRecoveryService.ResetPasswordAsync(user, model.NewPassword);
            return RedirectToAction("ResetPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}