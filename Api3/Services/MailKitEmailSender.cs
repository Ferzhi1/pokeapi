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

        await client.SendMailAsync(message);
    }
}
