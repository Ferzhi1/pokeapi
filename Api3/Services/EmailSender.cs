using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using api3.Models;
using System.Threading.Tasks;

public class EmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        Console.WriteLine($"[INFO] Intentando enviar correo a: {to}");

        using var client = new SmtpClient(_emailSettings.SmtpServer)
        {
            Port = _emailSettings.Port,
            Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
            EnableSsl = _emailSettings.EnableSsl
        };

        var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        try
        {
            Console.WriteLine("[INFO] Enviando correo...");
            await client.SendMailAsync(message);
            Console.WriteLine("[INFO] Correo enviado correctamente.");
        }
        catch (SmtpException ex)
        {
            Console.WriteLine($"[ERROR] SMTP Exception: {ex.Message}");
            Console.WriteLine($"[ERROR] Status Code: {ex.StatusCode}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] General Exception: {ex.Message}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
            }
        }

    }




}
