using api3.Models;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> RequestReset(string email)
    {
        var token = await _passwordRecoveryService.GenerateResetToken(email);
        if (token == null) return BadRequest("El correo no está registrado.");

        return Ok("Correo enviado con instrucciones.");
    }


    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        var model = new ResetPasswordModel
        {
            Token = token,
            Email = email
        };
        return View(model);
    }




    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        var success = await _passwordRecoveryService.ResetPassword(model);
        if (!success) return BadRequest("Token inválido o expirado.");

        return Ok("Contraseña restablecida correctamente.");
    }
}

