using Application.Common.Models;
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

            if (RegistrationHelpers.HasExpired(myUser.Value.RegisterTokenExpieryTime))
                return BadRequest("Token has been expired");

            // Keycloak requires both name parts; without them the account is created but can never
            // sign in. FullName is all the domain has, so split it.
            var (firstName, lastName) = RegistrationHelpers.SplitFullName(myUser.Value.FullName);

            var created = await _keycloakClient.CreateUserAsync(registerUserRequest.Username,
                                                                myUser.Value.UserEmail,
                                                                firstName,
                                                                lastName,
                                                                registerUserRequest.Password,
                                                                cancellationToken);

            if (created.IsFailure)
                return BadRequest(created.CustomError.error);

            string keycloakUserId;

            if (created.Value.AlreadyExisted)
            {
                // Registration is resumable rather than one-shot. An account can be left half made
                // — created before the profile fields were required, or created and then abandoned
                // when role assignment failed — and previously every retry died here on the 409,
                // so the user could never be repaired. Adopting it is the same trust level as the
                // reset flow: whoever holds this token received it at that email address.
                var repaired = await AdoptExistingUserAsync(registerUserRequest, myUser.Value.UserEmail,
                                                            firstName, lastName, cancellationToken);

                if (repaired.IsFailure)
                    return BadRequest(repaired.CustomError.error);

                keycloakUserId = repaired.Value;
            }
            else
            {
                keycloakUserId = created.Value.Id!;
            }

            // The old ASP.NET Identity path never assigned a role, so every self-registered user
            // came out with an empty role claim. Assign the domain user's type instead. This runs
            // for the adopted case too — it is exactly the step that may have failed last time.
            var roleAssigned = await _keycloakClient.AssignRealmRoleAsync(keycloakUserId,
                                                                         myUser.Value.UserTypeId.ToString(),
                                                                         cancellationToken);

            if (!roleAssigned.IsSuccess)
                return BadRequest(roleAssigned.Error);

            // keycloakUserId becomes the token's `sub`; without this link LoginController cannot
            // resolve the domain row and every login fails with "Not registered user".
            var linked = await _userregisterservice.UpdateUserAsync(myUser.Value.Id, keycloakUserId);

            if (!linked.IsSuccess)
                return BadRequest(linked.Error);

            // Burn the invite token so the link cannot be reused.
            await _userregisterservice.ExpireRegisterTokenAsync(myUser.Value.Id);

            return Ok();
        }


        /// <summary>
        /// Brings an account Keycloak already holds up to the state a fresh registration would have
        /// produced: the required name fields present, and the password the user just chose.
        /// Returns its Keycloak id.
        /// </summary>
        private async Task<CustomResult<string>> AdoptExistingUserAsync(RegisterUserRequest registerUserRequest,
                                                                        string userEmail,
                                                                        string firstName,
                                                                        string lastName,
                                                                        CancellationToken cancellationToken)
        {
            var existing = await _keycloakClient.FindUserAsync(registerUserRequest.Username, cancellationToken);

            if (existing.IsFailure)
                existing = await _keycloakClient.FindUserAsync(userEmail, cancellationToken);

            if (existing.IsFailure)
                return CustomResult<string>.Failure(CustomError.Failure("A user with that name or email already exists"));

            // Missing firstName/lastName is what stops the direct-access grant with "Account is not
            // fully set up", so this is the step that makes a pre-existing account able to sign in.
            var profileUpdated = await _keycloakClient.UpdateUserProfileAsync(existing.Value.Id, firstName, lastName, cancellationToken);

            if (!profileUpdated.IsSuccess)
                return CustomResult<string>.Failure(CustomError.Failure(profileUpdated.Error));

            var passwordSet = await _keycloakClient.ResetPasswordAsync(existing.Value.Id, registerUserRequest.Password, cancellationToken);

            if (!passwordSet.IsSuccess)
                return CustomResult<string>.Failure(CustomError.Failure(passwordSet.Error));

            return CustomResult<string>.Success(existing.Value.Id);
        }

    }
}
