using Application.Common.Interfaces;
using System.Security.Claims;
using TaskMaster.ServiceDefaults;

namespace WebApiAuth.Services;

public class CurrentUserService : ICurrentUserService
{
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        // Was ClaimTypes.GivenName, a claim no token this app has ever issued carried.
        UserName = user?.FindFirstValue(AuthenticationExtensions.NameClaimType);
    }

    public string UserId { get; }
    public string UserName { get; }
}
