using System.Net.Mail;

namespace WebApiEmailService.Services
{
    public class JobReportService : IJobReportService
    {
        private readonly ILogger _logger;

        public JobReportService(ILogger<JobReportService> logger)
        {
            _logger = logger;
        }

        record EmailRequest(string To, string? From, string Subject, string Body, bool IsBodyHtml = false);

        public async Task SendReportAsync()
        {
            try
            {
                EmailRequest req = new EmailRequest("st@gmail.com",
                                                    "st1@gmail.com",
                                                    "Test Subject",
                                                    "This is a test email body.",
                                                    false);

                var smtpClient = new SmtpClient("localhost", 25) // or 2525
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true,
                    EnableSsl = false
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("test@localhost.com"),
                    Subject = req.Subject,
                    Body = req.Body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(req.To);

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Recipient}", req.To);

                // Throws SocketException if cannot resolve
                // await System.Net.Dns.GetHostAddressesAsync(smtpHost);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                throw new InvalidOperationException(
                    $"SMTP host could not be resolved. " +
                    "Set a valid SMTP_HOST environment variable or ensure the 'papercut' service is running and resolvable.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"SMTP host  could not be resolved. " +
                    "Set", ex);
            }
        }
    }
}
