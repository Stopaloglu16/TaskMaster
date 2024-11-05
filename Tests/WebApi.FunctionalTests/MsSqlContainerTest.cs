using JetBrains.Annotations;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Testcontainers.MsSql;

namespace WebApi.FunctionalTests;


public abstract class MsSqlContainerTest : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;

    public MsSqlContainerTest(MsSqlContainer msSqlContainer)
    {
        _msSqlContainer = msSqlContainer;
    }

    // # --8<-- [start:UseMsSqlContainer]
    public Task InitializeAsync()
    {
        return _msSqlContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _msSqlContainer.DisposeAsync().AsTask();
    }

    [Fact]
    public void ConnectionStateReturnsOpen()
    {
        // Given
        using DbConnection connection = new SqlConnection(_msSqlContainer.GetConnectionString());

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
        var execResult = await _msSqlContainer.ExecScriptAsync(scriptContent)
            .ConfigureAwait(true);

        // Then
        Assert.True(0L.Equals(execResult.ExitCode), execResult.Stderr);
        Assert.Empty(execResult.Stderr);
    }
    // # --8<-- [end:UseMsSqlContainer]

    // # --8<-- [start:CreateMsSqlContainer]
    [UsedImplicitly]
    public sealed class MsSqlDefaultConfiguration : MsSqlContainerTest
    {
        public MsSqlDefaultConfiguration()
            : base(new MsSqlBuilder().Build())
        {
        }
    }
    // # --8<-- [end:CreateMsSqlContainer]
}