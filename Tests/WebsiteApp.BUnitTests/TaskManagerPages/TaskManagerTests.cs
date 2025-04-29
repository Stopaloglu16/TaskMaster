using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Blazored.LocalStorage;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using Radzen.Blazor;
using System.Security.Claims;
using WebsiteApp.BUnitTests.LoginPages;
using WebsiteApp.Components.Pages.TaskManagerPages;
using WebsiteApp.Services;

namespace WebsiteApp.BUnitTests.TaskManagerPages;

public class TaskManagerTests : TestContext
{

    [Fact]
    public void TaskManager_ModalValid_Test()
    {
        // Arrange: Register required services
        Services.AddScoped<IAuthService, AuthService>();
        Services.AddHttpClient();

        Services.AddScoped<IWebApiService<TaskListDto, TaskListDto>, WebApiService<TaskListDto, TaskListDto>>();
        Services.AddScoped<IWebApiService<TaskListFormRequest, TaskListFormRequest>, WebApiService<TaskListFormRequest, TaskListFormRequest>>();
        Services.AddScoped<IWebApiService<TaskListFormRequest, HttpResponseMessage>, WebApiService<TaskListFormRequest, HttpResponseMessage>>();

        Services.AddScoped<IWebApiService<SelectListItem, SelectListItem>, WebApiService<SelectListItem, SelectListItem>>();

        Services.AddSingleton<NotificationService>();

        // Use a mocked HttpClient for better test isolation
        var mockHttpClient = new HttpClient(new MockHttpMessageHandler());
        Services.AddSingleton(mockHttpClient);

       
        Services.AddBlazoredLocalStorage();

        // Mock AuthenticationState
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "testuser@example.com")
        }, "TestAuthentication"));

        var authState = new AuthenticationState(claimsPrincipal);
        var authenticationStateTask = Task.FromResult(authState);



        // Register the CascadingParameter
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthenticationStateProvider(authenticationStateTask));

        // Render the component
        var cut = RenderComponent<TaskManager>(parameters => parameters
            .AddCascadingValue(authenticationStateTask)
        );

        var gridCut = cut.FindComponent<RadzenDataGrid<TaskListDto>>();


        Assert.True(false);


    }


    // Mock HttpMessageHandler for HttpClient
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly bool _shouldSucceed;

        public MockHttpMessageHandler(bool shouldSucceed = true)
        {
            _shouldSucceed = shouldSucceed;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_shouldSucceed)
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"items\":[{\"id\":1,\"title\":\"task 1\",\"dueDate\":\"2025-01-18\",\"completedDate\":null,\"assignedTo\":\"taskuser\",\"taskItemCount\":3,\"taskItemCompletedCount\":0}],\"pageSize\":10,\"pageNumber\":1,\"totalPages\":1,\"totalCount\":1,\"hasPreviousPage\":false,\"hasNextPage\":false}")
                });
            }
            else
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.Unauthorized,
                    Content = new StringContent("{\"Username or password not correct\"}")
                });
            }
        }
    }
}
