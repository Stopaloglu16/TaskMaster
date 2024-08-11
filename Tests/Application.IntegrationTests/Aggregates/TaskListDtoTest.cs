using Application.Aggregates.TaskListAggregate.Commands.Create;
using Application.Aggregates.TaskListAggregate.Queries;
using SharedUtilityTestMethods;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IntegrationTests.Aggregates;

public class TaskListDtoTest
{

    const string MockTitle = "MockTitle";
    private ICollection<ValidationResult>? results = null;


    [Fact]
    public void CreateNewTaskListDto_Success()
    {
        //Arrange
        CreateTaskListRequest createTaskListRequest = new() { Title = MockTitle, DueDate = DateOnly.FromDateTime(new DateTime()) };

        //Assert
        Assert.NotNull(createTaskListRequest);
        Assert.NotNull(createTaskListRequest.Title);
        Assert.NotNull(createTaskListRequest?.DueDate);
        Assert.Null(createTaskListRequest.AssignedToId);
    }


    [Fact]
    public void CreateNewTaskListDto_OnlyTitle_Success()
    {
        //Arrange
        CreateTaskListRequest createTaskListRequest = new() { Title = MockTitle };

        //Assert
        Assert.NotNull(createTaskListRequest);
        Assert.NotNull(createTaskListRequest.Title);
    }


    [Fact]
    public void CreateNewTaskListDto_LongTitle_Fail()
    {
        var longMockTitle = TextGenerator.RandomString(101);

        //Arrange
        CreateTaskListRequest createTaskListRequest = new() { Title = longMockTitle };

        //Act
        var validateResult =  ValidateClass.Validate(createTaskListRequest, out results);
        var resultList = results.ToList();

        //Assert
        Assert.NotNull(createTaskListRequest);
        Assert.False(validateResult);
        Assert.Contains("100", resultList[0].ErrorMessage);
    }


}
