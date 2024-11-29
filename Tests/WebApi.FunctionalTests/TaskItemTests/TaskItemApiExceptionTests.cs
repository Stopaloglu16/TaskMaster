using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedTestDataLibrary.TaskDataSample;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApi.FunctionalTests.Helpers;

namespace WebApi.FunctionalTests.TaskItemTests;

public class TaskItemApiExceptionTests : BaseIntegrationTest
{

    private string token { get; set; }

    public TaskItemApiExceptionTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "YourAudience");

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    [Fact]
    public async Task CreateTaskItem_InValidTaskItem_SaveFail()
    {
        var mockTaskList = TaskListData.CreateCreateTaskListRequestValid();
        var response = await _httpClient.PostAsJsonAsync($"/api/v1.0/tasklist", mockTaskList);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        // Arrange
        var mockTaskItem = TaskItemData.CreateCreateTaskItemRequestEmpty();

        // Act
        var responseItem = await _httpClient.PostAsJsonAsync($"/api/v1.0/taskitem", mockTaskItem);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, responseItem.StatusCode);
    }


    [Fact]
    public async Task CreateTaskItem_InValidTaskListId_SaveFail()
    {
        var mockTaskList = TaskListData.CreateCreateTaskListRequestValid();
        var response = await _httpClient.PostAsJsonAsync($"/api/v1.0/tasklist", mockTaskList);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        // Arrange
        var mockTaskItem = TaskItemData.CreateCreateTaskItemRequestValid(66);

        // Act
        var response1 = await _httpClient.PostAsJsonAsync($"/api/v1.0/taskitem", mockTaskItem);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response1.StatusCode);
        var badRequestMessage = await response1.Content.ReadAsStringAsync();

        Assert.True(badRequestMessage?.Contains("not found"));
    }


    [Fact]
    public async Task CreateTaskItems_MaxTaskItem_SaveFail()
    {

        const int maxItemCount = 50;

        // Arrange
        TaskList taskList = new TaskList() { Title = "mockTitle" };
        await _dbContext.TaskLists.AddAsync(taskList);
        await _dbContext.SaveChangesAsync();

        var taskListMock = await _dbContext.TaskLists.ToListAsync();


        // Act
        for (int i = 0; i < maxItemCount; i++)
        {
            var mockTaskItem1 = TaskItemData.CreateCreateTaskItemRequestGenerator(1, i);

            var response = await _httpClient.PostAsJsonAsync($"/api/v1.0/taskitem", mockTaskItem1);

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            await Task.Delay(100);
        }

        var mockTaskItem = TaskItemData.CreateCreateTaskItemRequestGenerator(1, 51);
        var response1 = await _httpClient.PostAsJsonAsync($"/api/v1.0/taskitem", mockTaskItem);


        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response1.StatusCode);
        var badRequestMessage = await response1.Content.ReadAsStringAsync();

        //The task list reached to max task item
        Assert.True(badRequestMessage?.Contains("max"));
    }

}
