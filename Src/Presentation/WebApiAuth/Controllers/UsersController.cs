using Application.Aggregates.UserAggregate.Commands;
using Application.Common.Interfaces;
using Application.Common.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceLayer.Users;

namespace WebApiAuth.Controllers
{
    [ApiVersion(1)]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;
        private readonly IEmailSender _emailSender;

        public UsersController(IUserService userService,
                               IOptions<AppSettings> appSettings1,
                               IEmailSender emailSender)
        {
            _userService = userService;
            _appSettings = appSettings1.Value;
            _emailSender = emailSender;
        }


        [HttpGet("users")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Get(bool IsActive, int UserTypeId)
        {
            var userList = await _userService.GetUsers(IsActive, UserTypeId);

            if (userList != null)
                return Ok(userList);

            return BadRequest("asd");
        }

        [HttpGet("user/{Id}")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetById(int Id)
        {
            var userDto = await _userService.GetUserById(Id);

            if (userDto.IsFailure)
                return BadRequest("System issue");

            return Ok(userDto.Value);
        }


        [HttpPost]
        //[Authorize(Roles = "AdminUser")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post(CreateUserRequest createUserRequest)
        {
            try
            {
                var newUser = await _userService.AddUser(createUserRequest);

                if (newUser.IsSuccess)
                {
                    await _emailSender.SendRegisterEmailAsync(createUserRequest.UserEmail, createUserRequest.UserEmail, newUser.Value.ToString());
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}
