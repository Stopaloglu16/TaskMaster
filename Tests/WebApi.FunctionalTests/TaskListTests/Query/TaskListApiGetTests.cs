using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Aggregates.TaskListAggregate.Queries;
using Azure;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebApi.FunctionalTests.Helpers;

namespace WebApi.FunctionalTests.TaskListTests.Query;


public class TaskListApiGetTests : BaseIntegrationTest
{

    private string token { get; set; }
    private string apiVersion = "v1.0";

    public TaskListApiGetTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "YourAudience");

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    }


    //TODO Add more user scenarios
    // null task item
    // 1 task item
    // half completed
    // all completed


    [Fact]
    public async Task GetTaskList_ActiveTaskList_GetSuccess()
    {

        //TODO Add more user scenarios

        CancellationToken cancellationToken = new CancellationToken();

        //Arrange
        TaskList taskList = new TaskList() { Title = "mockTitle", DueDate = DateOnly.FromDateTime(DateTime.Now) };

        await _dbContext.TaskLists.AddAsync(taskList);
        await _dbContext.SaveChangesAsync(cancellationToken);


        TaskItem taskItem = new TaskItem() { Title = "mockTitle", Description = "lorem ipsumn", TaskListId = 1 };

        await _dbContext.TaskItems.AddAsync(taskItem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Act
        
        var response = await _httpClient.GetAsync($"/api/{apiVersion]}/tasklist");


        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<TaskListDto>>();


        Assert.True(result.Count > 0);     

    }
}

