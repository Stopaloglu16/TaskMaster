using Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using TaskMaster.ServiceDefaults;

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
        // Shaped like a real Keycloak access token so the tests exercise the claims production
        // actually sees: sub (mapped to NameIdentifier by the JwtBearer handler), preferred_username
        // for the name, and the flat `roles` claim the realm's role mapper emits.
        var mylist = new List<Claim>();

        mylist.Add(new Claim(ClaimTypes.NameIdentifier, "MockUserId"));
        mylist.Add(new Claim(AuthenticationExtensions.NameClaimType, "MockUserName"));

        mylist.Add(new Claim(AuthenticationExtensions.RoleClaimType, UserType.AdminUser.ToString()));
        mylist.Add(new Claim(JwtRegisteredClaimNames.Aud, "ExpectedAudience"));

        var identity = new ClaimsIdentity(mylist,
                                          "IntegrationTest",
                                          AuthenticationExtensions.NameClaimType,
                                          AuthenticationExtensions.RoleClaimType);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "IntegrationTest");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
