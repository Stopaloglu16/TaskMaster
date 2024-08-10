using Domain.Enums;

namespace Domain.UnitTests;

public class UserTypeTests
{
    [Theory]
    [InlineData(UserType.AdminUser)]
    public void AdminUser_EnumValue_Test(UserType userType)
    {
        Assert.Equal(0, Convert.ToInt32(userType));
    }

    [Theory]
    [InlineData(UserType.TaskUser)]
    public void TaskUser_EnumValue_Test(UserType userType)
    {
        Assert.Equal(1, Convert.ToInt32(userType));
    }

    [Theory]
    [InlineData(UserType.ReadOnly)]
    public void ReadOnly_EnumValue_Test(UserType userType)
    {
        Assert.Equal(2, Convert.ToInt32(userType));
    }

}