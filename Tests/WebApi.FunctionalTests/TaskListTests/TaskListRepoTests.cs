using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.FunctionalTests.Utility;

namespace WebApi.FunctionalTests.TaskListTests;

[TestCaseOrderer(
    ordererTypeName: "WebApi.FunctionalTests.Utility.PriorityOrderer",
    ordererAssemblyName: "WebApi.FunctionalTests")]
public class TaskListRepoTests : BaseIntegrationTest
{

    public TaskListRepoTests(IntegrationTestWebAppFactory factory) : base(factory)
    {

    }


    public static bool TestCreateCalled;
    public static bool TestUpdateCalled;
    public static bool TestRemoveCalled;



    [Fact, TestPriority(1)]
    public async Task CreateTaskList_ValidTaskList_SaveSuccess()
    {
        CancellationToken cancellationToken = new CancellationToken();

        //Arrange
        TaskList taskList = new TaskList() { Title = "mockTitle", DueDate = DateOnly.FromDateTime(DateTime.Now) };

        //Act
        await _dbContext.TaskLists.AddAsync(taskList);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var taskListList = await _dbContext.TaskLists.ToListAsync();

        //Assert
        Assert.Equal(1, taskListList.Count);
        Assert.Equal(1, taskListList[0].Id);

        TestCreateCalled = true;
    }


    [Fact, TestPriority(2)]
    public async Task UpdateTaskList_ValidTaskList_UpdateSuccess()
    {

        Assert.True(TestCreateCalled);

        CancellationToken cancellationToken = new CancellationToken();

        const string MockTitle = "mockUpdate";
        var MockDueDate = DateOnly.FromDateTime(DateTime.Now);

        //Arrange
        var taskListOrj = await _dbContext.TaskLists.FirstOrDefaultAsync(p => p.Id == 1);

        taskListOrj.Title = MockTitle;
        taskListOrj.DueDate = MockDueDate;

        //Act
        //await _dbContext.TaskLists.AddAsync(taskListOrj);
        await _dbContext.SaveChangesAsync(cancellationToken);


        //Assert
        var taskListAssert = await _dbContext.TaskLists.FirstOrDefaultAsync(p => p.Id == 1);

        TestUpdateCalled = true;
        Assert.Equal(MockTitle, taskListAssert.Title);
    }


    [Fact, TestPriority(3)]
    public async Task RemoveTaskList_ValidTaskList_RemoveSuccess()
    {
        Assert.True(TestUpdateCalled);

        //Arrange
        var taskListOrj = await _dbContext.TaskLists.FirstOrDefaultAsync(p => p.Id == 1);

        //Act
        _dbContext.Entry(taskListOrj).State = EntityState.Deleted;

        _dbContext.TaskLists.Remove(taskListOrj);
        await _dbContext.SaveChangesAsync();

        var taskListList = await _dbContext.TaskLists.ToListAsync();

        //Assert

        Assert.Equal(0, taskListList.Count);
    }

}
