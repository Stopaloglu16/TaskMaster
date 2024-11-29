using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Asp.Versioning;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using ServiceLayer.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiAuth.Models;

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

        //private readonly AppSettings _appSettings;

        private readonly IUserService _userloginservice;

        public LoginController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        //IOptions<AppSettings> appSettings1,
        IConfiguration iConfig,
        IOptions<JwtSettings> jwtsettings,
        IUserService userloginservice)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_appSettings = appSettings1.Value;
            _configuration = iConfig;
            _jwtsettings = jwtsettings.Value;
            _userloginservice = userloginservice;
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
                    return Unauthorized("Failed to login");


                RefreshToken refreshToken = GenerateRefreshToken();

                var aspUser = await _userManager.FindByNameAsync(loginRequest.Username);

                var webUser = await _userloginservice.GetUserByAspId(aspUser.Id);

                if (webUser.IsFailure)
                    return BadRequest("Not registered user");


                await _userloginservice.SaveRefreshTokenAsync(refreshToken, webUser.Value.Id);


                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginRequest.Username),
                    new Claim(ClaimTypes.Role, webUser.Value.UserType.ToString())
                };

                var LoginResponse = new UserLoginResponse();

                LoginResponse.RefreshToken = refreshToken.Token;
                LoginResponse.AccessToken = GenerateAccessToken(claims);
                LoginResponse.UserName = loginRequest.Username;


                return Ok(LoginResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private RefreshToken GenerateRefreshToken()
        {
            RefreshToken refreshToken = new RefreshToken();

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.Token = Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddDays(7);

            return refreshToken;
        }

        //private string GenerateAccessToken(string userId, string userName, UserType userType)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);

        //    var mylist = new List<Claim>();

        //    //mylist.Add(new Claim(ClaimTypes.Name, userId));

        //    mylist.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        //    mylist.Add(new Claim(ClaimTypes.GivenName, userName));
        //    mylist.Add(new Claim(ClaimTypes.Role, userType.ToString()));


        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(mylist),
        //        Expires = DateTime.UtcNow.AddDays(1),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
        //        SecurityAlgorithms.HmacSha256Signature)
        //    };


        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);
        //}


        private string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtsettings.SecretKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost:5001",
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: signinCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        }


        [MapToApiVersion(1)]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRefreshToken([FromBody] TokenRefreshRequest tokenRefreshRequest)
        {
            try
            {
                var refreshToken = await _userloginservice.GetRefreshToken(tokenRefreshRequest.RefreshToken);

                if (refreshToken == null || refreshToken.IsUsed || refreshToken.IsRevoked || refreshToken.ExpiryDate <= DateTime.UtcNow)
                    return Unauthorized("Invalid refresh token");

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtsettings.SecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false //the token's expiration date not important
                };


                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(tokenRefreshRequest.AccessToken, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return Unauthorized("Invalid token");


                var username = principal.Identity.Name; //this is mapped to the Name claim by default

                
                var newAccessToken = GenerateAccessToken(principal.Claims);
                var newRefreshToken = GenerateRefreshToken();

                var aspUser = await _userManager.FindByNameAsync(username);
                var webUser = await _userloginservice.GetUserByAspId(aspUser.Id);

                if (webUser.IsFailure)
                    return BadRequest("Not registered user");


                await _userloginservice.SaveRefreshTokenAsync(newRefreshToken, webUser.Value.Id);

                var LoginResponse = new UserLoginResponse();

                LoginResponse.RefreshToken = newRefreshToken.Token;
                LoginResponse.AccessToken = newAccessToken;
                LoginResponse.UserName = aspUser.UserName;

                return Ok(LoginResponse);

            }
            catch (Exception ex)
            {
                return Ok(new UserLoginResponse());
            }
        }




    }
}
