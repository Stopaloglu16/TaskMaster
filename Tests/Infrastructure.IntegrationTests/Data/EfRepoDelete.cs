using Domain.Entities;

namespace Infrastructure.IntegrationTests.Data;

public class EfRepoDelete : BaseEfRepo
{

    [Fact(DisplayName = "Repository Delete")]
    public async Task DeleteTaskList_Valid_Success()
    {
        //Arrange
        var mockTitle = "testContributor";

        var repository = GetRepository();
        var taskList = new TaskList() { Title = mockTitle };

        await repository.AddAsync(taskList);

        //Act
        await repository.DeleteAsync(taskList.Id);

        //Assert
        var deletedContributor = await repository.GetByIdAsync(taskList.Id);

        Assert.Null(deletedContributor);

    }
}