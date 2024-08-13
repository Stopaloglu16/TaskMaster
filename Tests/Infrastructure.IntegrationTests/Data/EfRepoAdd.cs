using Domain.Entities;

namespace Infrastructure.IntegrationTests.Data;

public class EfRepoAdd : BaseEfRepo
{

    [Fact(DisplayName = "Repository Add")]
    public async Task AddTaskList_Valid_Success()
    {
        //Arrange
        var mockTitle = "testContributor";

        var repository = GetRepository();
        var Contributor = new TaskList() { Title = mockTitle };

        //Act
        await repository.AddAsync(Contributor);

        //Assert
        var newContributor = (await repository.ListAllAsync()).FirstOrDefault();

        Assert.Equal(mockTitle, newContributor?.Title);
        Assert.True(newContributor?.Id > 0);
    }
}