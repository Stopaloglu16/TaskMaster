using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(EmailRequest request, CancellationToken cancellationToken);

    Task SendRegisterEmailAsync(string Username, string To, string Link, CancellationToken cancellationToken);

    Task SendForgotPasswordEmailAsync(string Username, string To, string Token, CancellationToken cancellationToken);
}
