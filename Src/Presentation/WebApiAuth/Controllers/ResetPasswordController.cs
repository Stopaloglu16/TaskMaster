using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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
        public async Task<IActionResult> Post(ResetPasswordRequest resetPasswordRequest)
        {
            var myUser = await _userregisterservice.GetUserByAsync(resetPasswordRequest.Email );

            if (myUser.IsFailure)
                return BadRequest("User not found");


            DateTime myNow = DateTime.UtcNow;
            int tt = myNow.Subtract(myUser.Value.RegisterTokenExpieryTime).Minutes;

            if (tt >= 15)
                return BadRequest("Token has been expired");


            var currentUser = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);

            if (currentUser is null)
                return BadRequest("User not registered");

            var result = await _userManager.ResetPasswordAsync(currentUser, resetPasswordRequest.ResetCode, resetPasswordRequest.NewPassword);

            if (result.Succeeded)
            {
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
