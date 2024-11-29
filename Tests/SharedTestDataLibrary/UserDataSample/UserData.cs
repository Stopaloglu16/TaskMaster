using Application.Aggregates.UserAggregate.Commands;
using Domain.Enums;

namespace SharedTestDataLibrary.UserDataSample;

public class UserData
{
    public static CreateUserRequest CreateUserRequestValidAdminSample()
    {
        return new CreateUserRequest() { FullName = "admin user", UserEmail = $"mock{UserType.AdminUser.ToString()}@hotmail.co.uk", UserType = UserType.AdminUser };
    }
}
