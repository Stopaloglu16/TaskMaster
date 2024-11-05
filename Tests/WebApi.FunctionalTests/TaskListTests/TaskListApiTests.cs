using Application.Aggregates.TaskListAggregate.Commands.Create;
using Application.Aggregates.TaskListAggregate.Queries;
using Newtonsoft.Json;
using SharedTestDataLibrary.TaskDataSample;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using WebApi.FunctionalTests.Helpers;
using WebApi.FunctionalTests.Utility;

namespace WebApi.FunctionalTests.TaskListTests;

[TestCaseOrderer(
    ordererTypeName: "WebApi.FunctionalTests.Utility.PriorityOrderer",
    ordererAssemblyName: "WebApi.FunctionalTests")]
public class TaskListApiTests : BaseIntegrationTest
{

    private string token { get; set; }

    public TaskListApiTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "YourAudience");

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    }


    [Fact, TestPriority(1)]
    public async Task CreateTaskList_ValidTaskList_SaveSuccess()
    {
        // Arrange
        var mockTaskList = TaskListData.CreateCreateTaskListRequestValid();


        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/v1.0/tasklist", mockTaskList);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        var response1 = await _httpClient.GetAsync($"/api/v1.0/tasklist");

        var personId = await response1.Content.ReadAsStringAsync();

    }


    [Fact, TestPriority(2)]
    public async Task UpdateTaskList_ValidTaskList_UpdateSuccess()
    {
        const string mockTitle = "New Mock";
        var mockDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));

        // Arrange
        var mockTaskList = TaskListData.CreateUpdateTaskListRequestEmpty();

        mockTaskList.Title = mockTitle;
        mockTaskList.DueDate = mockDueDate;

        // Act
        var response = await _httpClient.PutAsync($"/api/v1.0/tasklist/{mockTaskList.Id}", HttpHelper.GetJsonHttpContent(mockTaskList));

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var response1 = await _httpClient.GetAsync($"/api/v1.0/tasklist");
        var result = await response1.Content.ReadFromJsonAsync<List<TaskListDto>>();

        Assert.Equal(mockTitle, result[0].Title);
        Assert.Equal(mockDueDate, result[0].DueDate);
    }



    [Fact, TestPriority(3)]
    public async Task RemoveTaskList_ValidTaskList_RemoveSuccess()
    {

        // Arrange

        // Act
        var response = await _httpClient.DeleteAsync($"/api/v1.0/tasklist/1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        var response1 = await _httpClient.GetAsync($"/api/v1.0/tasklist");
        var result = await response1.Content.ReadFromJsonAsync<List<TaskListDto>>();

        Assert.Equal(0, result.Count);
    }

}
