using System.Collections.Concurrent;
using Application.Common.Interfaces;
using Application.Common.Models;

namespace WebApiAuth.FunctionalTests;

/// <summary>
/// Records what would have been emailed instead of sending it. The real EmailSender talks to SMTP
/// and Mailinator, neither of which exists in a test run — and the invite/reset token the tests need
/// is precisely what the mail carries, so capturing it here beats scraping an inbox.
/// </summary>
public sealed class FakeEmailSender : IEmailSender
{
    public sealed record SentEmail(string Username, string To, string Token);

    public ConcurrentBag<SentEmail> RegisterEmails { get; } = new();
    public ConcurrentBag<SentEmail> ForgotPasswordEmails { get; } = new();

    public Task SendEmailAsync(EmailRequest request, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task SendRegisterEmailAsync(string Username, string To, string Link, CancellationToken cancellationToken)
    {
        RegisterEmails.Add(new SentEmail(Username, To, Link));
        return Task.CompletedTask;
    }

    public Task SendForgotPasswordEmailAsync(string Username, string To, string Token, CancellationToken cancellationToken)
    {
        ForgotPasswordEmails.Add(new SentEmail(Username, To, Token));
        return Task.CompletedTask;
    }
}
