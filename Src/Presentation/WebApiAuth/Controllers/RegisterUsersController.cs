using Application.Aggregates.UserAuthAggregate;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Users;
using WebApiAuth.Services;

namespace WebApiAuth.Controllers
{
    [ApiVersion(1)]
    [Route("api/v{apiVersion:apiVersion}/registerusers")]
    [ApiController]
    public class RegisterUsersController : ControllerBase
    {
        private readonly IUserRegisterService _userregisterservice;
        private readonly IKeycloakClient _keycloakClient;

        public RegisterUsersController(IUserRegisterService userregisterservice,
                                       IKeycloakClient keycloakClient)
        {
            _userregisterservice = userregisterservice;
            _keycloakClient = keycloakClient;
        }


        [HttpPost]
        [ProducesResponseType(typeof(Ok), 200)]
        [ProducesResponseType(typeof(BadRequestResult), 400)]
        public async Task<IActionResult> Post(RegisterUserRequest registerUserRequest, CancellationToken cancellationToken)
        {
            // The domain row is created up front by an admin; registration only claims it with the
            // invite token. That part is unchanged.
            var myUser = await _userregisterservice.GetUserByAsync(registerUserRequest.Username, registerUserRequest.TokenConfirm);

            if (myUser.IsFailure)
                return BadRequest("User not found");

            DateTime myNow = DateTime.Now;
            int tt = myNow.Subtract(myUser.Value.RegisterTokenExpieryTime).Days;

            if (tt >= 1)
                return BadRequest("Token has been expired");

            var created = await _keycloakClient.CreateUserAsync(registerUserRequest.Username,
                                                                myUser.Value.UserEmail,
                                                                registerUserRequest.Password,
                                                                cancellationToken);

            if (created.IsFailure)
                return BadRequest(created.CustomError.error);

            // The old ASP.NET Identity path never assigned a role, so every self-registered user
            // came out with an empty role claim. Assign the domain user's type instead.
            var roleAssigned = await _keycloakClient.AssignRealmRoleAsync(created.Value,
                                                                         myUser.Value.UserTypeId.ToString(),
                                                                         cancellationToken);

            if (!roleAssigned.IsSuccess)
                return BadRequest(roleAssigned.Error);

            // created.Value is the Keycloak user id, which becomes the token's `sub`.
            await _userregisterservice.UpdateUserAsync(myUser.Value.Id, created.Value);

            return Ok();
        }

    }
}
