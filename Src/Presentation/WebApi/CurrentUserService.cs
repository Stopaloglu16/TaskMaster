using Application.Common.Interfaces;
using System.Security.Claims;
using TaskMaster.ServiceDefaults;

namespace WebApi
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            // Keycloak's `sub` — MapInboundClaims turns it into NameIdentifier. This is the value
            // stored in Domain User.AspId and stamped into the audit columns.
            UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Not ClaimTypes.Name: the token carries the username under preferred_username, which is
            // what AddDefaultAuthentication declares as the identity's NameClaimType.
            UserName = user?.FindFirstValue(AuthenticationExtensions.NameClaimType);
        }

        public string UserId { get; }
        public string UserName { get; }
    }
}
