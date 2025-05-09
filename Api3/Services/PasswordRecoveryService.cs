using api3.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class PasswordRecoveryService
{
    private readonly ApplicationDbContext _context;
    private readonly EmailSender _emailSender;

    public PasswordRecoveryService(ApplicationDbContext context, EmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task<string> GenerateResetToken(string email)
    {
        Console.WriteLine($"[INFO] Intentando generar token para el email: {email}");

        var user = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            Console.WriteLine("[ERROR] Usuario no encontrado.");
            return null;
        }

        var rawToken = Guid.NewGuid().ToString();
        Console.WriteLine($"[INFO] Token generado: {rawToken}");

        var hashedToken = BCrypt.Net.BCrypt.HashPassword(rawToken);
        Console.WriteLine("[INFO] Token hasheado y listo para guardar.");

        var tokenEntry = new PasswordResetToken
        {
            Email = email,
            TokenHash = hashedToken,
            ExpiryDate = DateTime.UtcNow.AddHours(1)
        };

        _context.passwordResetTokens.Add(tokenEntry);
        await _context.SaveChangesAsync();
        Console.WriteLine("[INFO] Token guardado en la base de datos.");

        await _emailSender.SendEmailAsync(email, "Recuperación de contraseña",
            $"<a href='https://tusitio.com/reset-password?token={rawToken}&email={email}'>Recuperar contraseña</a>");

        Console.WriteLine("[INFO] Correo enviado con éxito.");
        return rawToken;
    }

    public async Task<bool> ResetPassword(ResetPasswordModel model)
    {
        Console.WriteLine($"[INFO] Procesando restablecimiento para el email: {model.Email}");

        var tokenEntry = await _context.passwordResetTokens.FirstOrDefaultAsync(t => t.Email == model.Email && !t.Used);
        if (tokenEntry == null || tokenEntry.ExpiryDate < DateTime.UtcNow)
        {
            Console.WriteLine("[ERROR] Token inválido o expirado.");
            return false;
        }

        Console.WriteLine("[INFO] Token verificado, procediendo con la actualización de contraseña.");

        if (!BCrypt.Net.BCrypt.Verify(model.Token, tokenEntry.TokenHash))
        {
            Console.WriteLine("[ERROR] Token no coincide con el registrado.");
            return false;
        }

        var user = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
        {
            Console.WriteLine("[ERROR] Usuario no encontrado.");
            return false;
        }

        Console.WriteLine("[INFO] Usuario encontrado, actualizando contraseña...");
        user.Password = HashPassword(model.NewPassword);
        tokenEntry.Used = true;

        _context.UsuariosPokemonApi.Update(user);
        _context.passwordResetTokens.Update(tokenEntry);
        await _context.SaveChangesAsync();

        Console.WriteLine("[INFO] Contraseña actualizada correctamente.");
        return true;
    }

    private string HashPassword(string password)
    {
        Console.WriteLine("[INFO] Hasheando nueva contraseña.");
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
