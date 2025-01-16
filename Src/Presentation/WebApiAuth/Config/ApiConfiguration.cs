using Application.Common.Interfaces;
using Application.Repositories;
using Infrastructure.Repositories;
using ServiceLayer.Users;
using WebApiAuth.Services;

namespace WebApiAuth.Config;

public static class ApiConfiguration
{
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRegisterService, UserRegisterService>();
        services.AddScoped<IUserRegisterRepository, UserRegisterRepository>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
