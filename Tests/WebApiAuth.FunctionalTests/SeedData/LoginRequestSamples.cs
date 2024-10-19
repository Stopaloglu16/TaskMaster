using Application.Aggregates.UserAuthAggregate;
using Domain.Enums;

namespace WebApiAuth.FunctionalTests.SeedData
{
    internal class LoginRequestSamples
    {

        public static LoginRequest LoginRequestNullSample()
        {
            return new LoginRequest() { Username = null, Password = null };
        }


        public static LoginRequest LoginRequestValidSample(UserType userType = UserType.AdminUser)
        {
            return new LoginRequest() { Username = $"{userType.ToString()}@hotmail.co.uk", Password = "SuperStrongPassword+123" };
        }

    }
}
