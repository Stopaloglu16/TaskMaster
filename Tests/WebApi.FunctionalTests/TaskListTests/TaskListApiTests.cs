using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using SharedTestDataLibrary.TaskDataSample;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApi.FunctionalTests.Helpers;
using WebApi.FunctionalTests.Utility;

namespace WebApi.FunctionalTests.TaskListTests;

[TestCaseOrderer(
    ordererTypeName: "WebApi.FunctionalTests.Utility.PriorityOrderer",
    ordererAssemblyName: "WebApi.FunctionalTests")]
public class TaskListApiTests : BaseIntegrationTest
{

    private string token { get; set; }
    private string apiVersion = "v1.0";
    const int pageNumber = 1;
    const int ItemsPerPage = 10;

    public TaskListApiTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "Audience");


        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    [Fact, TestPriority(1)]
    public async Task CreateTaskList_ValidTaskList_SaveSuccess()
    {
        // Arrange
        var mockTaskList = TaskListData.CreateCreateTaskListRequestValid();

        //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("IntegrationTest"); // Use the test auth

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/{apiVersion}/tasklist", mockTaskList);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
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
        var response = await _httpClient.PutAsync($"/api/{apiVersion}/tasklist/{mockTaskList.Id}", HttpHelper.GetJsonHttpContent(mockTaskList));

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var response1 = await _httpClient.GetAsync($"/api/{apiVersion}/tasklist?PageNumber={pageNumber}&PageSize={ItemsPerPage}&OrderBy=Id&IsDescending=false");
        var result = await response1.Content.ReadFromJsonAsync<PagingResponse<TaskListDto>>();

        Assert.Equal(mockTitle, result.Items.ElementAt(0).Title);
        Assert.Equal(mockDueDate, result.Items.ElementAt(0).DueDate);
    }



    [Fact, TestPriority(3)]
    public async Task RemoveTaskList_ValidTaskList_RemoveSuccess()
    {

        // Arrange

        // Act
        var response = await _httpClient.DeleteAsync($"/api/{apiVersion}/tasklist/1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        var response1 = await _httpClient.GetAsync($"/api/{apiVersion}/tasklist?PageNumber={pageNumber}&PageSize={ItemsPerPage}&OrderBy=Id&IsDescending=false");
        var result = await response1.Content.ReadFromJsonAsync<PagingResponse<TaskListDto>>();

        Assert.Equal(0, result.TotalCount);
    }

}
