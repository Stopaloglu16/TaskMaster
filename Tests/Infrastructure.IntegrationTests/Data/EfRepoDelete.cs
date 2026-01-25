using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.IntegrationTests.Data;

public class EfRepoDelete : BaseEfRepo
{

    [Fact(DisplayName = "Repository Delete")]
    public async Task DeleteTaskList_Valid_Success()
    {
        //Arrange
        var mockTitle = "testContributor";

        var repository = GetRepository();
        var taskList = new TaskList() { Title = mockTitle,  Priority = TaskPriority.Medium };

        await repository.AddAsync(taskList);

        //Act
        await repository.DeleteAsync(taskList.Id);

        //Assert
        var deletedContributor = await repository.GetByIdAsync(taskList.Id);

        Assert.Null(deletedContributor);

    }
}