using Domain.Entities;
using Domain.Enums;

namespace SharedTestDataLibrary.UserDataSample;

public class UserEntityData
{
    public static User CreateUserAdmin()
    {
        return new User()
        {
            FullName = "admin user",
            UserEmail = $"mock{UserType.AdminUser.ToString()}@hotmail.co.uk",
            UserTypeId = UserType.AdminUser
        };
    }

    public static User CreateUserTask()
    {
        return new User()
        {
            FullName = "task user",
            UserEmail = $"mock{UserType.TaskUser.ToString()}@hotmail.co.uk",
            UserTypeId = UserType.TaskUser
        };
    }

    public static User CreateUserReadOnly()
    {
        return new User()
        {
            FullName = "readonly user",
            UserEmail = $"mock{UserType.ReadOnly.ToString()}@hotmail.co.uk",
            UserTypeId = UserType.ReadOnly
        };
    }

}
