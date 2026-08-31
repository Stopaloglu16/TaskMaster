using Application.Aggregates.UserAuthAggregate;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Users;
using WebApiAuth.Services;

namespace WebApiAuth.Controllers
{
    /// <summary>
    /// Completes a password reset from this app's own page. The counterpart to
    /// ForgotPasswordController, which mints the token and emails the link.
    /// </summary>
    [ApiVersion(1)]
    [Route("api/v{apiVersion:apiVersion}/resetpassword")]
    [ApiController]
    public class ResetPasswordController : ControllerBase
    {
        private readonly IUserRegisterService _userRegisterService;
        private readonly IKeycloakClient _keycloakClient;

        public ResetPasswordController(IUserRegisterService userRegisterService,
                                       IKeycloakClient keycloakClient)
        {
            _userRegisterService = userRegisterService;
            _keycloakClient = keycloakClient;
        }


        [HttpPost]
        [ProducesResponseType(typeof(Ok), 200)]
        [ProducesResponseType(typeof(BadRequestResult), 400)]
        public async Task<IActionResult> Post(ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken)
        {
            // Same token check registration does: the domain row is the one that proves the link is
            // genuine, and it is matched on email + token together.
            var myUser = await _userRegisterService.GetUserByAsync(resetPasswordRequest.Username,
                                                                   resetPasswordRequest.TokenConfirm);

            if (myUser.IsFailure)
                return BadRequest("User not found");

            if (RegistrationHelpers.HasExpired(myUser.Value.RegisterTokenExpieryTime))
                return BadRequest("Token has been expired");

            var keycloakUser = await _keycloakClient.FindUserAsync(myUser.Value.UserEmail, cancellationToken);

            if (keycloakUser.IsFailure)
                return BadRequest("User not registered");

            var reset = await _keycloakClient.ResetPasswordAsync(keycloakUser.Value.Id,
                                                                 resetPasswordRequest.Password,
                                                                 cancellationToken);

            if (!reset.IsSuccess)
                return BadRequest(reset.Error);

            // Burn the token so the link cannot be replayed.
            await _userRegisterService.ExpireRegisterTokenAsync(myUser.Value.Id);

            return Ok();
        }

    }
}
