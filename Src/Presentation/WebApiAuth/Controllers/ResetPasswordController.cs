using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceLayer.Users;

namespace WebApiAuth.Controllers
{
    [ApiVersion(1)]
    [Route("api/v{apiVersion:apiVersion}/resetpassword")]
    [ApiController]
    public class ResetPasswordController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IUserRegisterService _userregisterservice;

        public ResetPasswordController(
                         UserManager<IdentityUser> userManager,
                         IOptions<AppSettings> appSettings,
                         IConfiguration iConfig,
                         IUserRegisterService userregisterservice)
        {
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _configuration = iConfig;
            _userregisterservice = userregisterservice;
        }


        [HttpPost]
        [ProducesResponseType(typeof(Ok), 200)]
        [ProducesResponseType(typeof(BadRequestResult), 400)]
        public async Task<IActionResult> Post(RegisterUserRequest registerUserRequest)
        {
            throw new NotImplementedException("This method is not implemented yet.");

            var myUser = await _userregisterservice.GetUserByAsync(registerUserRequest.Username, registerUserRequest.TokenConfirm);

            if (myUser.IsFailure)
                return BadRequest("User not found");


            DateTime myNow = DateTime.Now;
            int tt = myNow.Subtract(myUser.Value.RegisterTokenExpieryTime).Days;

            if (tt >= 1)
                return BadRequest("Token has been expired");

            var myIduser = new IdentityUser { UserName = registerUserRequest.Username, Email = myUser.Value.UserEmail };

            //userRegister.Password = EncryptDecrypt.Decrypt(userRegister.Password, true, _appSettings.KeyEncrypte);
            myIduser.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(myIduser, registerUserRequest.Password);

            if (result.Succeeded)
            {
                await _userregisterservice.UpdateUserAsync(myUser.Value.Id, myIduser.Id);

                return Ok();
            }

            foreach (var error in result.Errors)
            {
                return BadRequest(error.Description);
            }

            return BadRequest("System issue");
        }

    }
}
