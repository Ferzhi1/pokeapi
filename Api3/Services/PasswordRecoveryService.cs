using api3.Models;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class PasswordRecoveryService
{
    private readonly ApplicationDbContext _context;
    private readonly EmailSender _emailSender; // ✅ Cambio de MailKitEmailSender a EmailSender

    public PasswordRecoveryService(ApplicationDbContext context, EmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task<string> GenerateResetToken(string email)
    {
        var user = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        var rawToken = Guid.NewGuid().ToString();
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(rawToken);

        var tokenEntry = new PasswordResetToken
        {
            Email = email,
            TokenHash = hashedToken,
            ExpiryDate = DateTime.UtcNow.AddHours(1)
        };

        _context.passwordResetTokens.Add(tokenEntry);
        await _context.SaveChangesAsync();

        await _emailSender.SendEmailAsync(email, "Recuperación de contraseña",
            $"<a href='https://tusitio.com/reset-password?token={rawToken}&email={email}'>Recuperar contraseña</a>");

        return rawToken;
    }

    public async Task<bool> ResetPassword(ResetPasswordModel model)
    {
        var tokenEntry = await _context.passwordResetTokens.FirstOrDefaultAsync(t => t.Email == model.Email && !t.Used);
        if (tokenEntry == null || tokenEntry.ExpiryDate < DateTime.UtcNow) return false;

        if (!BCrypt.Net.BCrypt.Verify(model.Token, tokenEntry.TokenHash)) return false;

        var user = await _context.UsuariosPokemonApi.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null) return false;

        user.Password = HashPassword(model.NewPassword);
        tokenEntry.Used = true;

        _context.UsuariosPokemonApi.Update(user);
        _context.passwordResetTokens.Update(tokenEntry);
        await _context.SaveChangesAsync();

        return true;
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
