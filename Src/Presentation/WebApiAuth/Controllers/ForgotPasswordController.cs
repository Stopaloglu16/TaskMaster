using Application.Aggregates.UserAuthAggregate;
using Application.Common.Interfaces;
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
    [Route("api/v{apiVersion:apiVersion}/forgotpassword")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordController(
                         UserManager<IdentityUser> userManager,
                         IOptions<AppSettings> appSettings,
                         IConfiguration iConfig,
                         IUserService userService,
                         IEmailSender emailSender)
        {
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _configuration = iConfig;
            _userService = userService;
            _emailSender = emailSender;
        }


        [HttpPost]
        [ProducesResponseType(typeof(Ok), 200)]
        [ProducesResponseType(typeof(BadRequestResult), 400)]
        public async Task<IActionResult> Post(ForgotPasswordRequest forgotPasswordRequest)
        {
            var aspUser = await _userManager.FindByEmailAsync(forgotPasswordRequest.Username);

            if (aspUser is null)
                return BadRequest("User not registered");

            // Generate the password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(aspUser);

            var myUser = await _userService.ForgotPassordAsync(forgotPasswordRequest.Username, resetToken);

            if (!myUser.IsSuccess)
                return BadRequest("User not found");

            await _emailSender.SendForgotPasswordEmailAsync(forgotPasswordRequest.Username, forgotPasswordRequest.Username, resetToken);

            return Ok();
        }

    }
}
