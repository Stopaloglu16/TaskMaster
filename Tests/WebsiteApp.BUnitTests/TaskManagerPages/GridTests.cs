using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using Radzen.Blazor;
using System.Security.Claims;
using WebsiteApp.BUnitTests.LoginPages;
using WebsiteApp.BUnitTests.Utilities;
using WebsiteApp.Components.Pages.TaskManagerPages;
using WebsiteApp.Services;

namespace WebsiteApp.BUnitTests.TaskManagerPages;

public class GridTests : TestContext
{
    private class TestWebApiService : IWebApiService<TaskListDto, TaskListDto>,
                                      IWebApiService<TaskListFormRequest, HttpResponseMessage>,
                                      IWebApiService<TaskListFormRequest, TaskListFormRequest>,
                                      IWebApiService<SelectListItem, SelectListItem>
    {
        private readonly List<TaskListDto> _data;


        public TestWebApiService(List<TaskListDto> data)
        {
            _data = data;
        }

        public async Task<PagingResponse<TaskListDto>> GetPagingDataAsync(string requestUri, bool requiresAuth = false)
        {
            // Parse the request URI to extract paging parameters  
            var uriFields = ParseUri.ParsePagingUrl(requestUri);

            // Extract page number and page size from the parsed URI fields  
            int pageNumber = uriFields.Item1;
            int pageSize = uriFields.Item2;

            // Apply paging to the IQueryable data source  
            IQueryable<TaskListDto> query = _data.AsQueryable()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // Create paging parameters object  
            PagingParameters pagingParameters = new PagingParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Return the paginated response  
            return await PagingResponse<TaskListDto>.CreateAsync(query, pagingParameters);
        }

        Task<List<TaskListDto>> IWebApiService<TaskListDto, TaskListDto>.GetAllDataAsync(string requestUri, bool requiresAuth)
        {
            return Task.FromResult(_data);
        }

        public Task<TaskListDto> GetDataByIdAsync(string requestUri, bool requiresAuth = false)
        {
            return Task.FromResult(_data.FirstOrDefault());
        }

        public Task<HttpResponseMessage> SaveAsync(string requestUri, TaskListDto obj, bool requiresAuth = false)
        {
            _data.Add(obj);
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }

        public Task<HttpResponseMessage> UpdateAsync(string requestUri, int Id, TaskListDto obj, bool requiresAuth = false)
        {
            var existing = _data.FirstOrDefault(x => x.Id == Id);
            if (existing != null)
            {
                _data.Remove(existing);
                _data.Add(obj);
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }

        public Task<HttpResponseMessage> DeleteAsync(string requestUri, int Id, bool requiresAuth = false)
        {
            var existing = _data.FirstOrDefault(x => x.Id == Id);
            if (existing != null)
            {
                _data.Remove(existing);
            }
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }

        Task<PagingResponse<HttpResponseMessage>> IWebApiService<TaskListFormRequest, HttpResponseMessage>.GetPagingDataAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }

        Task<List<HttpResponseMessage>> IWebApiService<TaskListFormRequest, HttpResponseMessage>.GetAllDataAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }

        Task<HttpResponseMessage> IWebApiService<TaskListFormRequest, HttpResponseMessage>.GetDataByIdAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> SaveAsync(string requestUri, TaskListFormRequest obj, bool requiresAuth = false)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> UpdateAsync(string requestUri, int Id, TaskListFormRequest obj, bool requiresAuth = false)
        {
            throw new NotImplementedException();
        }

        Task<PagingResponse<SelectListItem>> IWebApiService<SelectListItem, SelectListItem>.GetPagingDataAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }

        Task<List<SelectListItem>> IWebApiService<SelectListItem, SelectListItem>.GetAllDataAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }

        Task<SelectListItem> IWebApiService<SelectListItem, SelectListItem>.GetDataByIdAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> SaveAsync(string requestUri, SelectListItem obj, bool requiresAuth = false)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> UpdateAsync(string requestUri, int Id, SelectListItem obj, bool requiresAuth = false)
        {
            throw new NotImplementedException();
        }

        Task<PagingResponse<TaskListFormRequest>> IWebApiService<TaskListFormRequest, TaskListFormRequest>.GetPagingDataAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }

        Task<List<TaskListFormRequest>> IWebApiService<TaskListFormRequest, TaskListFormRequest>.GetAllDataAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }

        Task<TaskListFormRequest> IWebApiService<TaskListFormRequest, TaskListFormRequest>.GetDataByIdAsync(string requestUri, bool requiresAuth)
        {
            throw new NotImplementedException();
        }
    }

    private class TestNotificationService : NotificationService
    {
    }

    [Fact]
    public void RadzenGrid_Renders_Correctly()
    {
        JSInterop.SetupVoid("Radzen.preventArrows", _ => true);

        // Arrange
        var mockData = new List<TaskListDto>
        {
            new TaskListDto { Id = 1, Title = "Task 1", DueDate = DateOnly.FromDateTime(DateTime.Now), AssignedTo = "User A" },
            new TaskListDto { Id = 2, Title = "Task 2", DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), AssignedTo = "User B" }
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "testuser@example.com")
        }, "TestAuthentication"));

        var authState = new AuthenticationState(claimsPrincipal);
        var authenticationStateTask = Task.FromResult(authState);

        // Register the CascadingParameter
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthenticationStateProvider(authenticationStateTask));

        Services.AddSingleton<IWebApiService<TaskListDto, TaskListDto>>(new TestWebApiService(mockData));
        Services.AddSingleton<IWebApiService<TaskListFormRequest, HttpResponseMessage>>(new TestWebApiService(mockData));
        Services.AddSingleton<IWebApiService<TaskListFormRequest, TaskListFormRequest>>(new TestWebApiService(mockData));
        Services.AddSingleton<NotificationService>(new TestNotificationService());
        Services.AddSingleton<IWebApiService<SelectListItem, SelectListItem>>(new TestWebApiService(mockData));

        // Act
        var cut = RenderComponent<TaskManagerAdmin>(parameters => parameters
          .AddCascadingValue(authenticationStateTask)
        );

        // Assert
        var grid = cut.FindComponent<RadzenDataGrid<TaskListDto>>();
        Assert.NotNull(grid);

        var rows = cut.FindAll("tr");
        Assert.Equal(mockData.Count + 1, rows.Count); // +1 for the header row
    }

    [Fact]
    public void RadzenGrid_MultiPage_Correctly()
    {
        JSInterop.SetupVoid("Radzen.preventArrows", _ => true);

        // Arrange

        List<TaskListDto> mockData = new List<TaskListDto>();


        for (int i = 1; i < 16; i++)
        {
            mockData.Add(new TaskListDto { Id = i, Title = $"Task {i}", DueDate = DateOnly.FromDateTime(DateTime.Now), AssignedTo = $"User A{i}" });
        }


        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "testuser@example.com")
        }, "TestAuthentication"));

        var authState = new AuthenticationState(claimsPrincipal);
        var authenticationStateTask = Task.FromResult(authState);

        // Register the CascadingParameter
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthenticationStateProvider(authenticationStateTask));

        Services.AddSingleton<IWebApiService<TaskListDto, TaskListDto>>(new TestWebApiService(mockData));
        Services.AddSingleton<IWebApiService<TaskListFormRequest, HttpResponseMessage>>(new TestWebApiService(mockData));
        Services.AddSingleton<IWebApiService<TaskListFormRequest, TaskListFormRequest>>(new TestWebApiService(mockData));
        Services.AddSingleton<NotificationService>(new TestNotificationService());
        Services.AddSingleton<IWebApiService<SelectListItem, SelectListItem>>(new TestWebApiService(mockData));

        // Act
        var cut = RenderComponent<TaskManagerAdmin>(parameters => parameters
          .AddCascadingValue(authenticationStateTask)
        );

        // Assert
        var grid = cut.FindComponent<RadzenDataGrid<TaskListDto>>();
        Assert.NotNull(grid);

        var rows = cut.FindAll("tr");
        Assert.Equal(10+1, rows.Count); // +1 for the header row
    }


}
