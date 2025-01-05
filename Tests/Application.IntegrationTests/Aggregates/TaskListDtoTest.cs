
using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using SharedUtilityTestMethods;
using System.ComponentModel.DataAnnotations;

namespace Application.IntegrationTests.Aggregates;

public class TaskListDtoTest
{

    const string MockTitle = "MockTitle";
    private ICollection<ValidationResult>? results = null;


    [Fact]
    public void CreateNewTaskListDto_Success()
    {
        //Arrange
        TaskListFormRequest createTaskListRequest = new() { Title = MockTitle, DueDate = DateOnly.FromDateTime(new DateTime()) };

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
        TaskListFormRequest createTaskListRequest = new() { Title = MockTitle };

        //Assert
        Assert.NotNull(createTaskListRequest);
        Assert.NotNull(createTaskListRequest.Title);
    }


    [Fact]
    public void CreateNewTaskListDto_LongTitle_Fail()
    {
        var longMockTitle = TextGenerator.RandomString(101);

        //Arrange
        TaskListFormRequest createTaskListRequest = new() { Title = longMockTitle };

        //Act
        var validateResult = ValidateClass.Validate(createTaskListRequest, out results);
        var resultList = results.ToList();

        //Assert
        Assert.NotNull(createTaskListRequest);
        Assert.False(validateResult);
        Assert.Contains("100", resultList[0].ErrorMessage);
    }


}
