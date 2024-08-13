using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using SpotifyApi.Variables;

namespace SpotifyApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        public EmailService()
        {
        }

        public async Task SendEmailAsync(string email, string subject, string emailContent)
        {

            var useUsl = true;
            var senderName = Environment.GetEnvironmentVariable(EnvironmentVariables.SenderName);
            var senderEmail = Environment.GetEnvironmentVariable(EnvironmentVariables.SenderEmail);
            var senderEmailPassword = Environment.GetEnvironmentVariable(EnvironmentVariables.SenderEmailPassword);
            var smtpServer = Environment.GetEnvironmentVariable(EnvironmentVariables.SmtpServer);
            var smtpPort = Environment.GetEnvironmentVariable(EnvironmentVariables.SmtpPort);

            MimeMessage? emailMessage = new();

            emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = emailContent
            };

            using SmtpClient? client = new();
            await client.ConnectAsync(smtpServer, int.Parse(smtpPort), useUsl);
            await client.AuthenticateAsync(senderEmail, senderEmailPassword);
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
    }
}