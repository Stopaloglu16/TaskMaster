using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Data;

public class EfRepoAdd : BaseEfRepo
{


    [Fact]
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