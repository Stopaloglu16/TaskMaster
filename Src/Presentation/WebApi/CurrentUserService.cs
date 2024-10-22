using Application.Common.Interfaces;
using System.Security.Claims;

namespace WebApi
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            UserName = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName);
        }

        public string UserId { get; }
        public string UserName { get; }
    }
}
