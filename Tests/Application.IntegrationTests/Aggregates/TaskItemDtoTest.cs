using Application.Aggregates.TaskItemAggregate.Commands.Create;
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

public class TaskItemDtoTest
{

    const string MockTitle = "Mock title";
    const string MockDescription = "Mock description";
    private ICollection<ValidationResult>? results = null;

    [Fact]
    public void CreateNewTaskListDto_OnlyTitle_Success()
    {
        //Arrange
        CreateTaskItemRequest createTaskItemRequest = new CreateTaskItemRequest() { Title = MockTitle };
        

        //Assert
        Assert.NotNull(createTaskItemRequest);
        Assert.NotNull(createTaskItemRequest.Title);
    }


    [Fact]
    public void CreateNewTaskListDto_Success()
    {
        //Arrange
        CreateTaskItemRequest createTaskItemRequest = new CreateTaskItemRequest() { Title = MockTitle, Description = "" };

        //Assert
        Assert.NotNull(createTaskItemRequest);
        Assert.NotNull(createTaskItemRequest.Title);
    }

    [Fact]
    public void CreateNewTaskItemDto_LongTitle_Fail()
    {
        var longMockTitle = TextGenerator.RandomString(101);

        //Arrange
        CreateTaskItemRequest createTaskItemRequest = new CreateTaskItemRequest() { Title = longMockTitle, Description = "" };

        //Act
        var validateResult = ValidateClass.Validate(createTaskItemRequest, out results);
        var resultList = results.ToList();

        //Assert
        Assert.NotNull(createTaskItemRequest);
        Assert.False(validateResult);
        Assert.Contains("100", resultList[0].ErrorMessage);
    }

}
