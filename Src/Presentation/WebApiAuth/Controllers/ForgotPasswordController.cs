using Application.Aggregates.UserAuthAggregate;
using Application.Common.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Users;
using WebApiAuth.Services;

namespace WebApiAuth.Controllers
{
    [ApiVersion(1)]
    [Route("api/v{apiVersion:apiVersion}/forgotpassword")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly IKeycloakClient _keycloakClient;
        private readonly IUserService _userService;
        private readonly IUserRegisterService _userRegisterService;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordController(IKeycloakClient keycloakClient,
                                        IUserService userService,
                                        IUserRegisterService userRegisterService,
                                        IEmailSender emailSender)
        {
            _keycloakClient = keycloakClient;
            _userService = userService;
            _userRegisterService = userRegisterService;
            _emailSender = emailSender;
        }


        [HttpPost]
        [ProducesResponseType(typeof(Ok), 200)]
        [ProducesResponseType(typeof(BadRequestResult), 400)]
        public async Task<IActionResult> Post(ForgotPasswordRequest forgotPasswordRequest, CancellationToken cancellationToken)
        {
            // The reset page is ours, not Keycloak's hosted one, so this mints our own token and
            // emails a link to it. Keycloak is only consulted to confirm the account exists — the
            // password itself is set later, through the Admin API, by ResetPasswordController.
            var keycloakUser = await _keycloakClient.FindUserAsync(forgotPasswordRequest.Username, cancellationToken);

            if (keycloakUser.IsFailure)
                return BadRequest("User not registered");

            // The link has to carry the email, because that is what the token is stored against.
            var email = keycloakUser.Value.Email ?? forgotPasswordRequest.Username;

            var domainUser = await _userRegisterService.GetUserByAsync(email);

            if (domainUser.IsFailure)
                return BadRequest("User not registered");

            // Reuses the invite token: a fresh GUID with a two-hour expiry.
            var resetToken = await _userService.RefreshRegisterToken(domainUser.Value.Id);

            if (resetToken.IsFailure)
                return BadRequest(resetToken.CustomError.error);

            await _emailSender.SendForgotPasswordEmailAsync(email, email, resetToken.Value.ToString(), cancellationToken);

            return Ok();
        }

    }
}
