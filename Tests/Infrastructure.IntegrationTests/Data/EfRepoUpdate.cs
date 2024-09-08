using Domain.Entities;

namespace Infrastructure.IntegrationTests.Data;

public class EfRepoUpdate : BaseEfRepo
{

    [Fact(DisplayName = "Repository Update")]
    public async Task UpdateTaskList_Valid_Success()
    {
        //Arrange
        const string mockTitle = "testContributor";
        const string mockUpdatedTitle = "test updated Contributor";

        var repository = GetRepository();
        var Contributor = new TaskList() { Title = mockTitle };

        await repository.AddAsync(Contributor);

        var newContributor = await repository.GetByIdAsync(Contributor.Id);

        newContributor.Title = mockUpdatedTitle;


        //Act
        await repository.UpdateAsync(newContributor);


        //Assert
        var updatedContributor = (await repository.ListAllAsync()).FirstOrDefault();

        Assert.Equal(mockUpdatedTitle, updatedContributor?.Title);
        Assert.True(updatedContributor?.Id > 0);
    }
}