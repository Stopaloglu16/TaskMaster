using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using SharedUtilityTestMethods;
using System.ComponentModel.DataAnnotations;

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
        TaskItemFormRequest createTaskItemRequest = new TaskItemFormRequest() { Title = MockTitle, TaskListId = 0 };


        //Assert
        Assert.NotNull(createTaskItemRequest);
        Assert.NotNull(createTaskItemRequest.Title);
    }


    [Fact]
    public void CreateNewTaskListDto_Success()
    {
        //Arrange
        TaskItemFormRequest createTaskItemRequest = new TaskItemFormRequest() { Title = MockTitle, Description = "", TaskListId = 0 };

        //Assert
        Assert.NotNull(createTaskItemRequest);
        Assert.NotNull(createTaskItemRequest.Title);
    }

    [Fact]
    public void CreateNewTaskItemDto_LongTitle_Fail()
    {
        var longMockTitle = TextGenerator.RandomString(101);

        //Arrange
        TaskItemFormRequest createTaskItemRequest = new TaskItemFormRequest() { Title = longMockTitle, Description = "", TaskListId = 0 };

        //Act
        var validateResult = ValidateClass.Validate(createTaskItemRequest, out results);
        var resultList = results.ToList();

        //Assert
        Assert.NotNull(createTaskItemRequest);
        Assert.False(validateResult);
        Assert.Contains("100", resultList[0].ErrorMessage);
    }

}
