using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAggregate.Queries;
using WebApp.Handlers;
using WebApp.Services;

namespace WebApp.Config;

public static class WebConfiguration
{

    public static IServiceCollection AddBlazorServices(this IServiceCollection services)
    {

        //services.AddHttpClient<IWebApiService<User, User>, WebApiService<User, User>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        services.AddHttpClient<IWebApiService<TaskListDto, TaskListDto>, WebApiService<TaskListDto, TaskListDto>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        services.AddHttpClient<IWebApiService<TaskListFormRequest, TaskListFormRequest>, WebApiService<TaskListFormRequest, TaskListFormRequest>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        services.AddHttpClient<IWebApiService<TaskListFormRequest, HttpResponseMessage>, WebApiService<TaskListFormRequest, HttpResponseMessage>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        services.AddHttpClient<IWebApiService<UserDto, UserDto>, WebApiService<UserDto, UserDto>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        services.AddHttpClient<IWebApiService<UserFormRequest, UserFormRequest>, WebApiService<UserFormRequest, UserFormRequest>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        services.AddHttpClient<IWebApiService<UserFormRequest, HttpResponseMessage>, WebApiService<UserFormRequest, HttpResponseMessage>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        services.AddHttpClient<IWebApiService<CreateUserRequest, CreateUserRequest>, WebApiService<CreateUserRequest, CreateUserRequest>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        services.AddHttpClient<IWebApiService<UpdateUserRequest, UpdateUserRequest>, WebApiService<UpdateUserRequest, UpdateUserRequest>>().AddHttpMessageHandler<ValidateHeaderHandler>();



        return services;
    }

}
