using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(EmailRequest request);

    Task SendRegisterEmailAsync(string Username, string To, string Link);

    Task SendForgotPasswordEmailAsync(string Username, string To, string Link);
}
