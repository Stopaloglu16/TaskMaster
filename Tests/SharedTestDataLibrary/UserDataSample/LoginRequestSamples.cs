using Application.Aggregates.UserAuthAggregate;
using Domain.Enums;

namespace SharedTestDataLibrary.UserDataSample;

public class LoginRequestSamples
{

    public static UserLoginRequest LoginRequestEmpty()
    {
        return new UserLoginRequest() { Username = string.Empty, Password = string.Empty };
    }


    public static UserLoginRequest CreateLoginRequestValidSample(UserType userType = UserType.AdminUser)
    {
        return new UserLoginRequest() { Username = $"{userType.ToString()}@hotmail.co.uk", Password = "SuperStrongPassword+123" };
    }

    public static UserLoginRequest CreateLoginRequestInValidSample(UserType userType = UserType.AdminUser)
    {
        return new UserLoginRequest() { Username = $"{userType.ToString()}@hotmail.co.uk", Password = "NotCorrectPassword@123" };
    }
}
