using System.Security.Claims;
using System.Text.Encodings.Web;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApi.FunctionalTests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                           ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
                            : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var mylist = new List<Claim>();

        mylist.Add(new Claim(ClaimTypes.Name, "TestUser"));
        mylist.Add(new Claim(ClaimTypes.NameIdentifier, "MockUserId"));
        mylist.Add(new Claim(ClaimTypes.GivenName, "MockUserName"));

        mylist.Add(new Claim(ClaimTypes.Role, UserType.AdminUser.ToString()));

        //var identity = new ClaimsIdentity(Array.Empty<Claim>(), "Test");
        var identity = new ClaimsIdentity(mylist, "IntegrationTest");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "IntegrationTest");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
