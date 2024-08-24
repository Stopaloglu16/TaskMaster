using Application.Aggregates.UserAuthAggregate;
using Asp.Versioning;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceLayer.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiAuth.Models;

namespace WebApiAuth.Controllers
{

    [ApiVersion(1)]
    [Route("api/v{v:apiVersion}/[controller]")]
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
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            if (ModelState.IsValid)
            {
                //user.Password = UtilityClass.Decrypt(user.Password, true, _appSettings.KeyEncrypted);

                var myResult = await _signInManager.PasswordSignInAsync(loginRequest.Username, loginRequest.Password, true, lockoutOnFailure: true);

                if (myResult.Succeeded)
                {
                    RefreshToken refreshToken = GenerateRefreshToken();

                    //var tempUser1 = await _userManager.FindByNameAsync(loginRequest.Username);

                    //var myuser = await _userloginservice.GetUserById(tempUser1.Id);

                    //myuser.myRoles.Add(new Role() { Id = 1, RoleName = "AdminRole" });

                    ////await _userloginservice.SaveRefreshTokenAsync(refreshToken, myuser.Id);
                    ////_userloginservice.SaveLoginLogAsync

                    //var LoginResponse = new LoginResponse();



                    //LoginResponse.AccessToken = GenerateAccessToken(myuser.AspId, myuser.UserName, myuser.myRoles);

                    //return Ok(LoginResponse);
                    return Ok();
                }
                else
                {
                    return Unauthorized("Failed to login");
                }
            }
            else
            {
                return BadRequest("Not valid");
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
            refreshToken.ExpiryDate = DateTime.UtcNow.AddMonths(6);

            return refreshToken;
        }

        private string GenerateAccessToken(string userId, string userName, ICollection<Role> userroles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);

            var mylist = new List<Claim>();

            //mylist.Add(new Claim(ClaimTypes.Name, userId));

            mylist.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            mylist.Add(new Claim(ClaimTypes.GivenName, userName));

            foreach (Role item in userroles)
            {
                mylist.Add(new Claim(ClaimTypes.Role, item.RoleName));
            }


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(mylist),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
