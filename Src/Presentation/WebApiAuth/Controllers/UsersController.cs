using Application.Aggregates.UserAggregate.Commands;
using Application.Common.Interfaces;
using Application.Common.Models;
using Asp.Versioning;
using Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceLayer.Users;
using WebApiAuth.Services;

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
        private readonly IKeycloakClient _keycloakClient;

        public UsersController(IUserService userService,
                               IOptions<AppSettings> appSettings1,
                               IEmailSender emailSender,
                               IKeycloakClient keycloakClient)
        {
            _userService = userService;
            _appSettings = appSettings1.Value;
            _emailSender = emailSender;
            _keycloakClient = keycloakClient;
        }


        [HttpGet("users")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Get(bool IsActive, UserType UserType)
        {
            var userList = await _userService.GetUsers(IsActive, UserType);

            if (userList != null)
                return Ok(userList);

            return BadRequest("Not found users");
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
        public async Task<ActionResult> Post(CreateUserRequest createUserRequest, CancellationToken cancellationToken)
        {
            try
            {
                var newUser = await _userService.AddUser(createUserRequest);

                if (newUser.IsSuccess)
                {
                    await _emailSender.SendRegisterEmailAsync(createUserRequest.UserEmail, createUserRequest.UserEmail, newUser.Value.ToString(), cancellationToken);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Backs the UserManager grid's Edit button, which PUTs to api/v1.0/users/{id}. The route
        /// existed on the client long before it existed here — the stub below was commented out —
        /// so saving an edit silently did nothing.
        /// </summary>
        [HttpPut("{Id:int}")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Put(int Id, UpdateUserRequest updateUserRequest)
        {
            var updated = await _userService.UpdateUser(Id, updateUserRequest);

            if (!updated.IsSuccess)
                return BadRequest(updated.Error);

            return Ok();
        }


        /// <summary>
        /// Removes a user: soft-deletes the domain row (cascading to the task lists they own) and
        /// then deletes the Keycloak account, so a removed user cannot still authenticate.
        /// </summary>
        [HttpDelete("user/{Id:int}")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Delete(int Id, CancellationToken cancellationToken)
        {
            // Domain row first, deliberately. If the Keycloak call then fails the user is already
            // hidden and blocked at LoginController's "Not registered user" check; the other order
            // would leave a visible user who can no longer sign in.
            var deleted = await _userService.SoftDeleteUserById(Id);

            if (deleted.IsFailure)
                return BadRequest(deleted.CustomError.error);

            // No AspId means they never completed registration, so there is no realm account.
            if (string.IsNullOrWhiteSpace(deleted.Value))
                return Ok();

            var keycloakDeleted = await _keycloakClient.DeleteUserAsync(deleted.Value, cancellationToken);

            if (!keycloakDeleted.IsSuccess)
                return BadRequest($"User removed, but the Keycloak account could not be deleted: {keycloakDeleted.Error}");

            return Ok();
        }


        [HttpPost("refreshregister")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post(int userId, string userEmail, CancellationToken cancellationToken)
        {
            try
            {
                var newUser = await _userService.RefreshRegisterToken(userId);

                if (newUser.IsSuccess)
                {
                    await _emailSender.SendRegisterEmailAsync(userEmail, userEmail, newUser.Value.ToString(), cancellationToken);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[HttpPost]
        ////[Authorize(Roles = "AdminUser")]
        //[ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult> Put(CreateUserRequest createUserRequest)
        //{
        //    try
        //    {
        //        var newUser = await _userService. (createUserRequest);

        //        if (newUser.IsSuccess)
        //        {
        //            await _emailSender.SendRegisterEmailAsync(createUserRequest.UserEmail, createUserRequest.UserEmail, newUser.Value.ToString());
        //        }

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        [HttpGet("taskuserselectlist")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetTaskUserSelectList()
        {
            var userList = await _userService.GetTaskUserSelectList();

            if (userList != null)
                return Ok(userList);

            return BadRequest("Not found users");
        }


        [HttpGet("userlist")]
        [ProducesResponseType(typeof(Ok), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetUserList([FromQuery] PagingParameters pagingParameters, CancellationToken cancellationToken)
        {
            var userList = await _userService.GetActiveUsersWithPagination(pagingParameters, cancellationToken);

            if (userList != null)
                return Ok(userList);

            return BadRequest("Not found users");
        }

    }

}
