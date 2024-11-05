using Application.Aggregates.UserAuthAggregate;
using Domain.Enums;

namespace SharedTestDataLibrary.UserDataSample;

public class LoginRequestSamples
{

    public static LoginRequest LoginRequestEmpty()
    {
        return new LoginRequest() { Username = string.Empty, Password = string.Empty };
    }


    public static LoginRequest LoginRequestValidSample(UserType userType = UserType.AdminUser)
    {
        return new LoginRequest() { Username = $"{userType.ToString()}@hotmail.co.uk", Password = "SuperStrongPassword+123" };
    }

}
