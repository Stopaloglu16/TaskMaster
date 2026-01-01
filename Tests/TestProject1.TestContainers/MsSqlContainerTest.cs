using DotNet.Testcontainers.Builders;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Testcontainers.MsSql;
using Xunit.Abstractions;


//using Microsoft.Data.SqlClient;
//using Renci.SshNet;
//using System;
//using System.Threading.Tasks;
//using Testcontainers.MsSql;
//using Xunit;

namespace TestProject1.TestContainers;


public class DatabaseTests : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;

    public DatabaseTests()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithPassword("Your_strong_password123!")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task Can_Insert_And_Read_Data_From_SqlServer()
    {
        var connectionString = _msSqlContainer.GetConnectionString();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Create table
        var createTableCommand = new SqlCommand(
            "CREATE TABLE Users (Id INT PRIMARY KEY, Name NVARCHAR(100));",
            connection);
        await createTableCommand.ExecuteNonQueryAsync();

        // Insert data
        var insertCommand = new SqlCommand(
            "INSERT INTO Users (Id, Name) VALUES (1, 'Alice');",
            connection);
        await insertCommand.ExecuteNonQueryAsync();

        // Read data
        var selectCommand = new SqlCommand(
            "SELECT Name FROM Users WHERE Id = 1;",
            connection);

        var result = (string)await selectCommand.ExecuteScalarAsync();

        Assert.Equal("Alice", result);
    }
}
