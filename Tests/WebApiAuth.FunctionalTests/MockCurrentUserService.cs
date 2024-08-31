using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WebApiAuth.FunctionalTests;

public class MockCurrentUserService : ICurrentUserService
{
    public MockCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        UserId = "MockUserId";
        UserName = " MockUserName";
    }

    public string UserId { get; }
    public string UserName { get; }
}


public static class MockAdminUser
{
    public static string UserEmail = "taskmaster@hotmail.co.uk";
    public static string Password = "SuperStrongPassword+123";
}

