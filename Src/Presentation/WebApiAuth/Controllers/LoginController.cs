using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Users;
using WebApiAuth.Services;

namespace WebApiAuth.Controllers
{
    [ApiVersion(1)]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userloginservice;
        private readonly IAuthService _authService;

        public LoginController(IUserService userloginservice,
                               IAuthService authService)
        {
            _userloginservice = userloginservice;
            _authService = authService;
        }

        [MapToApiVersion(1)]
        [HttpPost("login")]
        [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(UserLoginRequest loginRequest, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not valid");

            // Keycloak's direct-access grant does the password check, and its brute-force detection
            // replaces the IdentityOptions lockout we used to configure here.
            var loginResponse = await _authService.LoginAsync(loginRequest.Username, loginRequest.Password, cancellationToken);

            if (loginResponse.IsFailure)
                return Unauthorized(loginResponse.CustomError.error);

            // The token's subject is the Keycloak user id; the domain row is linked to it by AspId.
            var subject = _authService.GetSubject(loginResponse.Value.AccessToken);

            if (string.IsNullOrWhiteSpace(subject))
                return BadRequest("Token did not carry a subject");

            var webUser = await _userloginservice.GetUserByAspId(subject);

            if (webUser.IsFailure)
                return BadRequest("Not registered user");

            return Ok(loginResponse.Value);
        }


        [MapToApiVersion(1)]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRefreshToken([FromBody] RefreshTokenRequest tokenRefreshRequest, CancellationToken cancellationToken)
        {
            if (tokenRefreshRequest is null || string.IsNullOrWhiteSpace(tokenRefreshRequest.RefreshToken))
                return BadRequest("Invalid client request");

            // Keycloak owns refresh-token rotation and expiry now (SSO session idle timeout), so the
            // RefreshToken/RefreshTokenExpiryTime columns on the domain User are no longer consulted.
            var loginResponse = await _authService.RefreshTokensAsync(tokenRefreshRequest.RefreshToken, cancellationToken);

            if (loginResponse.IsFailure)
                return Unauthorized(loginResponse.CustomError.error);

            return Ok(loginResponse.Value);
        }

    }
}
