using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using SpotifyApi.Utilities;
using SpotifyApi.Variables;

namespace SpotifyApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailService(IErrorHandlingService errorHandlingService) : IEmailService
    {
        private readonly IErrorHandlingService _errorHandlingService = errorHandlingService;

        public async Task SendEmailAsync(string email, string subject, string emailContent)
        {
            try
            {
                var useSsl = true;
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
                await client.ConnectAsync(smtpServer, int.Parse(smtpPort), useSsl);
                await client.AuthenticateAsync(senderEmail, senderEmailPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception exception)
            {
                var logErrorAction = "send email";

                _errorHandlingService.HandleError(
                    exception,
                    ErrorType.Internal,
                    logErrorAction
                );

                throw;
            }
        }
    }
}