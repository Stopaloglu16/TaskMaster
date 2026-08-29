using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Microsoft.EntityFrameworkCore;
using SharedTestDataLibrary.TaskDataSample;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApi.FunctionalTests.Helpers;
using WebApi.FunctionalTests.Utility;

namespace WebApi.FunctionalTests.TaskListTests;


[TestCaseOrderer(
    ordererTypeName: "WebApi.FunctionalTests.Utility.PriorityOrderer",
    ordererAssemblyName: "WebApi.FunctionalTests")]
public class TaskListBulkApiTests : BaseIntegrationTest
{

    private string token { get; set; }
    private string apiVersion = "v1.0";

    public TaskListBulkApiTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "Audience");

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    [Fact, TestPriority(1)]
    public async Task CreateTaskList_ValidTaskList_SaveSuccess()
    {
        // Arrange
        int[] taskCount = { 2, 3 }; // Task list count, Task item count

        const string taskUserName = "TaskUser1";

        var userDt = await _dbContext.Users.AddAsync(new Domain.Entities.User
        {
            FullName = taskUserName,
            UserEmail = "asd@gmail.com",
            UserTypeId = Domain.Enums.UserType.TaskUser,
            AspId = "TaskUser1AspId",
            RegisterToken = Guid.NewGuid(),
            RegisterTokenExpieryTime = DateTime.UtcNow.AddDays(1),
            Created = DateTime.UtcNow,
            CreatedBy = "System",
            IsDeleted = 0
        });

        await _dbContext.SaveChangesAsync();

        
        var mockTaskList = TaskListRequestData.CreateTaskListRequestEmpty(taskCount, taskUserName);

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/{apiVersion}/tasklist/bulk", mockTaskList);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<CreateTaskListResponse>>();

        var taskListCount = await _dbContext.TaskLists.ToListAsync();

        var mockItemCount = mockTaskList.Sum(x => x.createTaskItemRequests.Count());
        Assert.True(mockItemCount == result?.Count(), $"Api return compare {mockItemCount} {result?.Count()}");
        Assert.True(taskCount[0] == taskListCount.Count(), $"Db compare {taskCount[0]} {taskListCount.Count()}");
    }

}
