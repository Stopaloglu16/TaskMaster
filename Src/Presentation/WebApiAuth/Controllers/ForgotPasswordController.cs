using Application.Aggregates.UserAuthAggregate;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApiAuth.Services;

namespace WebApiAuth.Controllers
{
    [ApiVersion(1)]
    [Route("api/v{apiVersion:apiVersion}/forgotpassword")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly IKeycloakClient _keycloakClient;

        public ForgotPasswordController(IKeycloakClient keycloakClient)
        {
            _keycloakClient = keycloakClient;
        }


        [HttpPost]
        [ProducesResponseType(typeof(Ok), 200)]
        [ProducesResponseType(typeof(BadRequestResult), 400)]
        public async Task<IActionResult> Post(ForgotPasswordRequest forgotPasswordRequest, CancellationToken cancellationToken)
        {
            // Keycloak owns the reset token and hosts the set-password page, so there is nothing for
            // us to generate, store or email. The realm's SMTP settings point at the Papercut
            // container, so the mail is readable from the Aspire dashboard in development.
            var user = await _keycloakClient.FindUserByEmailAsync(forgotPasswordRequest.Username, cancellationToken);

            if (user.IsFailure)
                return BadRequest("User not registered");

            var sent = await _keycloakClient.SendUpdatePasswordEmailAsync(user.Value.Id, cancellationToken);

            if (!sent.IsSuccess)
                return BadRequest(sent.Error);

            return Ok();
        }

    }
}
