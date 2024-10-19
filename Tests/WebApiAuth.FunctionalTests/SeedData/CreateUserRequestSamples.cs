using Application.Aggregates.UserAggregate.Commands;
using Domain.Enums;

namespace WebApiAuth.FunctionalTests.SeedData
{
    internal class CreateUserRequestSamples
    {

        public static CreateUserRequest CreateUserRequestValidAdminSample()
        {
            return new CreateUserRequest() { FullName = "admin user", UserEmail = $"mock{UserType.AdminUser.ToString()}@hotmail.co.uk", UserType = UserType.AdminUser };
        }

    }
}
