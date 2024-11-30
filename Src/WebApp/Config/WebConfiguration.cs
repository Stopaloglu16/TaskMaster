using Application.Aggregates.TaskListAggregate.Queries;
using WebApp.Handlers;
using WebApp.Services;

namespace WebApp.Config;

public static class WebConfiguration
{

    public static IServiceCollection AddBlazorServices(this IServiceCollection services)
    {

        //services.AddHttpClient<IWebApiService<User, User>, WebApiService<User, User>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        services.AddHttpClient<IWebApiService<TaskListDto, TaskListDto>, WebApiService<TaskListDto, TaskListDto>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<CreateBranchRequest, CreateBranchResponse>, WebApiService<CreateBranchRequest, CreateBranchResponse>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        //services.AddHttpClient<IWebApiService<UpdateBranchRequest, UpdateBranchResponse>, WebApiService<UpdateBranchRequest, UpdateBranchResponse>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<CreateCarModelRequest, CreateCarModelResponse>, WebApiService<CreateCarModelRequest, CreateCarModelResponse>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        //services.AddHttpClient<IWebApiService<UpdateCarModelRequest, UpdateCarModelResponse>, WebApiService<UpdateCarModelRequest, UpdateCarModelResponse>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<CarDto, CarDto>, WebApiService<CarDto, CarDto>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<CreateCarRequest, CreateCarResponse>, WebApiService<CreateCarRequest, CreateCarResponse>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        //services.AddHttpClient<IWebApiService<UpdateCarRequest, UpdateCarResponse>, WebApiService<UpdateCarRequest, UpdateCarResponse>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<CarExtraDto, CarExtraDto>, WebApiService<CarExtraDto, CarExtraDto>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<CarModelDto, CarModelDto>, WebApiService<CarModelDto, CarModelDto>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<CreateCarHireCommand, int>, WebApiService<CreateCarHireCommand, int>>().AddHttpMessageHandler<ValidateHeaderHandler>();


        //services.AddHttpClient<IWebApiService<CarHireCarDto, CarHireCarDto>, WebApiService<CarHireCarDto, CarHireCarDto>>().AddHttpMessageHandler<ValidateHeaderHandler>();
        //services.AddHttpClient<IWebApiService<CarHireBookDisplay, CarHireBookDisplay>, WebApiService<CarHireBookDisplay, CarHireBookDisplay>>().AddHttpMessageHandler<ValidateHeaderHandler>();

        //services.AddHttpClient<IWebApiService<SelectListItem, SelectListItem>, WebApiService<SelectListItem, SelectListItem>>().AddHttpMessageHandler<ValidateHeaderHandler>();


        return services;
    }

}
