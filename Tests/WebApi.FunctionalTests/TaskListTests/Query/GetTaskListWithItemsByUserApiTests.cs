using Application.Aggregates.TaskListAggregate.Queries;
using Application.Aggregates.UserAggregate.Commands;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebApi.FunctionalTests.Helpers;

namespace WebApi.FunctionalTests.TaskListTests.Query;

public class GetTaskListWithItemsByUserApiTests : BaseIntegrationTest
{

    private string token { get; set; }
    private string apiVersion = "v1.0";
    const int pageNumber = 1;
    const int ItemsPerPage = 10;

    public GetTaskListWithItemsByUserApiTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "YourAudience");

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    [Fact]
    public async Task GetTaskList_ActiveTaskList_GetSuccess()
    {
        // Arrange
        var userId = await ArrangeDb();

        // Act
        var response = await _httpClient.GetAsync($"/api/{apiVersion}/tasklist/TaskListwithItemsByUserId/{userId}");

    
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<TaskListWithItemsDto>>();


        Assert.True(result.Count == 2);
    }

    public async Task<string> ArrangeDb()
    {
        CancellationToken cancellationToken = new CancellationToken();


        User user = new User( ) {
            FullName = "mockName",
            UserTypeId = UserType.TaskUser,
            UserEmail = "mockEmail@gamil.com",
            AspId = "mockAspId",
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var taskUserId = user.Id;

        // null task item
        TaskList taskList = new TaskList() { Title = "task1", DueDate = DateOnly.FromDateTime(DateTime.Now), AssignedToId = taskUserId };
        await _dbContext.TaskLists.AddAsync(taskList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 1 task item
        taskList = new TaskList() { Title = "task2", DueDate = DateOnly.FromDateTime(DateTime.Now) };

        TaskItem taskItem = new TaskItem() { Title = "mockTitle", Description = "lorem ipsumn" };
        taskList.TaskItems.Add(taskItem);

        await _dbContext.TaskLists.AddAsync(taskList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // half completed
        taskList = new TaskList() { Title = "task3", DueDate = DateOnly.FromDateTime(DateTime.Now), AssignedToId = taskUserId };

        for (int i = 0; i < 2; i++)
        {
            taskItem = new TaskItem() { Title = "mockTitle" + i.ToString(), Description = "lorem ipsumn" };
            taskList.TaskItems.Add(taskItem);
        }

        for (int i = 3; i < 5; i++)
        {
            taskItem = new TaskItem() { Title = "mockTitle" + i.ToString(), Description = "lorem ipsumn", IsCompleted = true };
            taskList.TaskItems.Add(taskItem);
        }

        await _dbContext.TaskLists.AddAsync(taskList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // all completed
        taskList = new TaskList() { Title = "task4", DueDate = DateOnly.FromDateTime(DateTime.Now), AssignedToId = taskUserId };

        for (int i = 0; i < 4; i++)
        {
            taskItem = new TaskItem() { Title = "mockTitle" + i.ToString(), Description = "lorem ipsumn", IsCompleted = true };
            taskList.TaskItems.Add(taskItem);
        }

        await _dbContext.TaskLists.AddAsync(taskList);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return "mockAspId";
    }

}