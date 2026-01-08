using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Mail;

namespace WebApi.Apis
{
    public static class EmailApi
    {
        public static RouteGroupBuilder EmailApiV1(this RouteGroupBuilder group)
        {
            // Route for query task lists
            group.MapPost("/SendEmail", SendEmail);

            return group;
        }

        record EmailRequest(string To, string? From, string Subject, string Body, bool IsBodyHtml = false);

        public static async Task<Results<Ok, BadRequest<string>>> SendEmail(CancellationToken cancellationToken)
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

                await smtpClient.SendMailAsync(mailMessage, cancellationToken);

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

            return TypedResults.Ok();
        }
    }
}
