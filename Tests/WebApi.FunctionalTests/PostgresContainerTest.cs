using Npgsql;
using System.Data;
using System.Data.Common;
using Testcontainers.PostgreSql;
using JetBrains.Annotations;

namespace WebApi.FunctionalTests;


public abstract class PostgresContainerTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public PostgresContainerTest(PostgreSqlContainer postgreSqlContainer)
    {
        _postgreSqlContainer = postgreSqlContainer;
    }

    // # --8<-- [start:UsePostgreSqlContainer]
    public Task InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }

    [Fact]
    public void ConnectionStateReturnsOpen()
    {
        // Given
        using DbConnection connection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());

        // When
        connection.Open();

        // Then
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task ExecScriptReturnsSuccessful()
    {
        // Given
        const string scriptContent = "SELECT 1;";

        // When
        var execResult = await _postgreSqlContainer.ExecScriptAsync(scriptContent)
            .ConfigureAwait(true);

        // Then
        Assert.True(0L.Equals(execResult.ExitCode), execResult.Stderr);
        Assert.Empty(execResult.Stderr);
    }
    // # --8<-- [end:UsePostgreSqlContainer]

    // # --8<-- [start:CreatePostgreSqlContainer]
    [UsedImplicitly]
    public sealed class PostgreSqlDefaultConfiguration : PostgresContainerTest
    {
        public PostgreSqlDefaultConfiguration()
            : base(new PostgreSqlBuilder().Build())
        {
        }
    }

    // # --8<-- [end:CreatePostgreSqlContainer]
}
