using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceLayer.Users;
using System.Security.Claims;
using WebApiAuth.Models;
using WebApiAuth.Services;

namespace WebApiAuth.Controllers
{

    [ApiVersion(1)]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly JwtSettings _jwtsettings;
        private IConfiguration _configuration;

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IUserService _userloginservice;
        private readonly IAuthService _authService;


        public LoginController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration iConfig,
        IOptions<JwtSettings> jwtsettings,
        IUserService userloginservice,
        IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = iConfig;
            _jwtsettings = jwtsettings.Value;
            _userloginservice = userloginservice;
            _authService = authService;
        }

        [MapToApiVersion(1)]
        [HttpPost("login")]
        [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(UserLoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not valid");

            //user.Password = UtilityClass.Decrypt(user.Password, true, _appSettings.KeyEncrypted);

            try
            {
                var myResult = await _signInManager.PasswordSignInAsync(loginRequest.Username,
                                                                                   loginRequest.Password,
                                                                                   isPersistent: true,
                                                                                   lockoutOnFailure: true);

                if (!myResult.Succeeded)
                    return Unauthorized("Username or password not correct");

                var aspUser = await _userManager.FindByNameAsync(loginRequest.Username);

                var webUser = await _userloginservice.GetUserByAspId(aspUser.Id);
                if (webUser.IsFailure)
                    return BadRequest("Not registered user");


                UserTokenDto userTokenDto = new UserTokenDto()
                {
                    UserGuidId = webUser.Value.UserGuidId,
                    UserId = webUser.Value.Id,
                    Role = webUser.Value.UserType.ToString(),
                    Username = webUser.Value.UserEmail
                };

                var loginResponse = await _authService.LoginAsync(userTokenDto);

                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [MapToApiVersion(1)]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRefreshToken([FromBody] RefreshTokenRequest tokenRefreshRequest)
        {
            try
            {
                if (tokenRefreshRequest is null)
                    return BadRequest("Invalid client request");

                string accessToken = tokenRefreshRequest.AccessToken;
                string refreshToken = tokenRefreshRequest.RefreshToken;

                var principal = _authService.GetPrincipalFromExpiredToken(tokenRefreshRequest.AccessToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                var userGuidIdClaim = userIdClaim?.Value;

                if (string.IsNullOrEmpty(userGuidIdClaim))
                {
                    return Unauthorized("User ID not found in token claims");
                }


                var userGuidId = Guid.Parse(userGuidIdClaim);
                var user = await _userloginservice.CheckRefreshTokenOfUser(userGuidId, refreshToken);

                if (!user.isSuccess)
                    return BadRequest(user.error);


                var webUser = await _userloginservice.GetUserByUserGuidId(userGuidId);

                UserTokenDto userTokenDto = new UserTokenDto()
                {
                    UserGuidId = userGuidId,
                    UserId = webUser.Value.Id,
                    Role = webUser.Value.UserType.ToString(),
                    Username = webUser.Value.UserEmail
                };


                //var loginResponse = await _authService.LoginAsync(userTokenDto);
                var loginResponse = _authService.RefreshTokensAsync(userTokenDto);

                return Ok(loginResponse);

            }
            catch (Exception ex)
            {
                return Ok(new UserLoginResponse());
            }
        }




    }
}
