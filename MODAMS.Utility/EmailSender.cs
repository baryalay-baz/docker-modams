using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace MODAMS.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(new MailboxAddress("MOD Asset Management System", "assetmanagement.mod@gmail.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            using (var emailClient = new SmtpClient())
            {
                try
                {
                    emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    emailClient.Authenticate("assetmanagement.mod@gmail.com", "tzndpilrdkxnwmxk");
                    emailClient.Send(emailToSend);
                    emailClient.Disconnect(true);

                    _logger.LogInformation($"Email sent to {email} with subject '{subject}'.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email to {email} with subject '{subject}'.");
                }
            }
            return Task.CompletedTask;
        }
    }
}
