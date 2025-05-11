using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskItemAggregate.Commands.Update;
using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Asp.Versioning;
using Domain.Entities;
using Google.Protobuf.WellKnownTypes;
using SharedTestDataLibrary.TaskDataSample;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApi.FunctionalTests.Helpers;
using WebApi.FunctionalTests.Utility;

namespace WebApi.FunctionalTests.TaskItemTests;

[TestCaseOrderer(
    ordererTypeName: "WebApi.FunctionalTests.Utility.PriorityOrderer",
    ordererAssemblyName: "WebApi.FunctionalTests")]
public class TaskItemApiTests : BaseIntegrationTest
{

    private string token { get; set; }
    private string apiVersion = "v1.0";
    const int pageNumber = 1;
    const int ItemsPerPage = 10;

    public TaskItemApiTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "YourAudience");

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    [Fact, TestPriority(1)]
    public async Task CreateTaskItem_ValidTaskItem_SaveSuccess()
    {
        var mockTaskList = TaskListData.CreateCreateTaskListRequestValid();
        var response = await _httpClient.PostAsJsonAsync($"/api/v1.0/tasklist", mockTaskList);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        // Arrange
        var mockTaskItem = TaskItemData.CreateCreateTaskItemRequestValid(1);

        // Act
        var responseItem = await _httpClient.PostAsJsonAsync($"/api/v1.0/taskitem", mockTaskItem);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, responseItem.StatusCode);
    }


    [Fact, TestPriority(2)]
    public async Task UpdateTaskItem_ValidTaskItem_UpdateSuccess()
    {
        const string mockTitle = "New Mock";
        const string mockDescription = "New Mock Description";

        // Arrange
        var mockTaskItem = TaskItemData.CreateUpdateTaskItemRequestValid(mockTitle, mockDescription);

        // Act
        var response = await _httpClient.PutAsync($"/api/v1.0/taskitem/1", HttpHelper.GetJsonHttpContent(mockTaskItem));

        // Assert
        Assert.True(System.Net.HttpStatusCode.OK == response.StatusCode, $"Put taskitem {response.StatusCode}");

        var response1 = await _httpClient.GetAsync($"/api/v1.0/taskitem/1");
        Assert.Equal(System.Net.HttpStatusCode.OK, response1.StatusCode);

        var result = await response1.Content.ReadFromJsonAsync<TaskItemFormRequest>();

        Assert.Equal(mockTitle, result.Title);
        Assert.Equal(mockDescription, result.Description);
    }


    [Fact, TestPriority(3)]
    public async Task CompleteTaskItem_ValidTaskItem_CompleteSuccess()
    {

        // Arrange
        CompleteTaskItemRequest completeTaskItemRequest = new CompleteTaskItemRequest(taskListId: 1, taskItemId: 1);

        // Act
        var response = await _httpClient.PatchAsJsonAsync($"/api/v1.0/taskitem/CompleteSingleItem", completeTaskItemRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var response1 = await _httpClient.GetAsync($"/api/v1.0/tasklist/GetTaskList/1");
        Assert.Equal(System.Net.HttpStatusCode.OK, response1.StatusCode);

        var result = await response1.Content.ReadFromJsonAsync<TaskListDto>();

        Assert.Equal(1, result.TaskItemCompletedCount);
    }


    [Fact, TestPriority(4)]
    public async Task RemoveTaskItem_ValidTaskItem_RemoveSuccess()
    {
        // Arrange

        // Act
        var response = await _httpClient.DeleteAsync($"/api/v1.0/taskitem/1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        var response1 = await _httpClient.GetAsync($"/api/v1.0/taskitem/1");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response1.StatusCode);
    }

}
