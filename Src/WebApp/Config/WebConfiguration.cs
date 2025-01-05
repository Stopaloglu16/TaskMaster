using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
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


        //services.AddHttpClient<IWebApiService<CarHireCarDto, CarHireCarDto>, WebApiService<CarHireCarDto, CarHireCarDto>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        //services.AddHttpClient<IWebApiService<CarHireBookDisplay, CarHireBookDisplay>, WebApiService<CarHireBookDisplay, CarHireBookDisplay>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<SelectListItem, SelectListItem>, WebApiService<SelectListItem, SelectListItem>>().AddHttpMessageHandler<ValidateHeaderHandler>();


        return services;
    }

}
