using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Greenix.Application.Interfaces;

namespace Greenix.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            // Logging email transmission is standard for development/tracing environments.
            _logger.LogInformation("==================================================================");
            _logger.LogInformation("EMAIL TRANSMISSION DISPATCHED:");
            _logger.LogInformation("Recipient: {Recipient}", to);
            _logger.LogInformation("Subject:   {Subject}", subject);
            _logger.LogInformation("Body Content:\n{Body}", body);
            _logger.LogInformation("==================================================================");

            return Task.CompletedTask;
        }
    }
}
