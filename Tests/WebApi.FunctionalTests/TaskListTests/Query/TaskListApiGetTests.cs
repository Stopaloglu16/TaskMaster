using Application.Aggregates.TaskListAggregate.Queries;
using Application.Common.Models;
using Domain.Entities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApi.FunctionalTests.Helpers;

namespace WebApi.FunctionalTests.TaskListTests.Query;


public class TaskListApiGetTests : BaseIntegrationTest
{

    private string token { get; set; }
    private string apiVersion = "v1.0";
    const int pageNumber = 1;
    const int ItemsPerPage = 10;

    public TaskListApiGetTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "YourAudience");

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    [Fact]
    public async Task GetTaskList_ActiveTaskList_GetSuccess()
    {
        // Arrange
        await ArrangeDb();

        // Act
        var response = await _httpClient.GetAsync($"/api/{apiVersion}/tasklist?PageNumber={pageNumber}&PageSize={ItemsPerPage}&OrderBy=Id&IsDescending=false");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagingResponse<TaskListDto>>();


        Assert.True(result.Items.Count > 0);

        foreach (var taskList in result.Items)
        {
            if (taskList.Title == "task1")
            {
                Assert.Equal(0, taskList.TaskItemCount);
                Assert.Equal(0, taskList.TaskItemCompletedCount);
            }
            else if (taskList.Title == "task2")
            {
                Assert.Equal(1, taskList.TaskItemCount);
                Assert.Equal(0, taskList.TaskItemCompletedCount);
            }
            else if (taskList.Title == "task3")
            {
                Assert.Equal(4, taskList.TaskItemCount);
                Assert.Equal(2, taskList.TaskItemCompletedCount);
            }
            else if (taskList.Title == "task4")
            {
                Assert.Equal(4, taskList.TaskItemCount);
                Assert.Equal(4, taskList.TaskItemCompletedCount);
            }

        }
    }

    public async Task ArrangeDb()
    {
        CancellationToken cancellationToken = new CancellationToken();

        // null task item
        TaskList taskList = new TaskList() { Title = "task1", DueDate = DateOnly.FromDateTime(DateTime.Now) };
        await _dbContext.TaskLists.AddAsync(taskList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 1 task item
        taskList = new TaskList() { Title = "task2", DueDate = DateOnly.FromDateTime(DateTime.Now) };

        TaskItem taskItem = new TaskItem() { Title = "mockTitle", Description = "lorem ipsumn" };
        taskList.TaskItems.Add(taskItem);

        await _dbContext.TaskLists.AddAsync(taskList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // half completed
        taskList = new TaskList() { Title = "task3", DueDate = DateOnly.FromDateTime(DateTime.Now) };

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
        taskList = new TaskList() { Title = "task4", DueDate = DateOnly.FromDateTime(DateTime.Now) };

        for (int i = 0; i < 4; i++)
        {
            taskItem = new TaskItem() { Title = "mockTitle" + i.ToString(), Description = "lorem ipsumn", IsCompleted = true };
            taskList.TaskItems.Add(taskItem);
        }

        await _dbContext.TaskLists.AddAsync(taskList);

        await _dbContext.SaveChangesAsync(cancellationToken);

    }

}

