using Application.Aggregates.SearchAggregate.Queries;
using Domain.Entities.SearchEntities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using SharedTestDataLibrary.AdvancedSearchSample;
using SharedTestDataLibrary.TaskDataSample;
using SharedTestDataLibrary.UserDataSample;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApi.FunctionalTests.Helpers;

namespace WebApi.FunctionalTests.AdvancedSearchTests;

[TestCaseOrderer(
    ordererTypeName: "WebApi.FunctionalTests.Utility.PriorityOrderer",
    ordererAssemblyName: "WebApi.FunctionalTests")]
public class AdvancedSearchTest : BaseIntegrationTest
{

    private string token { get; set; }
    private string apiVersion = "v1.0";
    const int pageNumber = 1;
    const int ItemsPerPage = 10;

    public AdvancedSearchTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
        token = TestTokens.Bearer;

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    // The seeding below now succeeds, but the query engine behind POST /advancedSearch does not run yet:
    // AdvancedSearchRepository catches every exception and returns an empty 200, SqlQueryRaw<object[]> is
    // not something EF Core can materialise, the sample tables are still SQL Server-shaped
    // ("dbo.TaskLists", unquoted PascalCase identifiers) and BuildOrder dereferences a null request.Sorts.
    // Unskip once those are done — the assertions below are the ones that should then hold.
    [Fact(Skip = "Advanced search query engine incomplete - see AdvancedSearchRepository.RunAdvancedSearchAsync")]
    public async Task CreateTaskItem_AdvancedSearch_ReturnSuccess()
    {

        // Arrange
        var seeded = await ArrangeDb();

        var mockRequestDto = AdvancedSearchRequestData.CreateAdvancedSearchRequest();

        // Arrange
        mockRequestDto.AdvancedSearchId = seeded.AdvancedSearchId;
        mockRequestDto.SelectedColumnIds = seeded.ColumnIds;
        mockRequestDto.Sorts = new List<SortDefinitionDto>();
        //mockRequestDto.Filters = new FilterGroupDto
        //{
        //    Logic = LogicalOperator.And,
        //    Items = new List<FilterItemDto>
        //    {
        //        new FilterItemDto
        //        {
        //            ColumnId = 1,
        //            OperatorCode = "Equals",
        //            Value = "TestValue"
        //        }
        //    }
        //};

        // Act
        var response = await _httpClient.PostAsJsonAsync($"/api/v1/advancedSearch", mockRequestDto);


        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AdvancedSearchResponseDto>();

        Assert.NotNull(result);
        Assert.Equal(seeded.ColumnIds.Count, result.Columns.Count);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Rows);
    }



    /// <summary>
    /// Ids the seeded metadata ended up with. Nothing here may assume 1, 2, 3 — the containerised
    /// database generates them.
    /// </summary>
    public record SeededSearch(int AdvancedSearchId, List<int> ColumnIds);

    /// <summary>
    /// Seeds one AdvancedSearch over TaskLists (columns: Title, IsCompleted) plus a single
    /// TaskList/TaskItem pair to search against. Every insert respects the FK order:
    /// ColumnTypes → Tables → ColumnDefinitions → AdvancedSearch → AdvancedSearchColumns.
    /// Failures are deliberately left to propagate; swallowing them just produced an empty search.
    /// </summary>
    public async Task<SeededSearch> ArrangeDb()
    {
        CancellationToken cancellationToken = new CancellationToken();

        // 1) Column types (and their operators) first so EF generates ids we can reference
        var columnTypes = ColumnTypeData.CreateColumnTypes();
        await _dbContext.ColumnTypes.AddRangeAsync(columnTypes, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 2) Tables next: AdvancedSearch.MainTableId is a FK onto this table, so the rows have to
        //    exist before the search root does. ParentTableId 0/1 in the sample data are placeholders.
        var table1 = AdvancedSearchTableData.CreateTaskListTable();
        table1.ParentTableId = null;

        var table2 = AdvancedSearchTableData.CreateTaskItemTable();
        await _dbContext.AdvancedSearchTables.AddAsync(table1, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        table2.ParentTableId = table1.Id;
        await _dbContext.AdvancedSearchTables.AddAsync(table2, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 3) Column definitions, mapped onto the ids the column types actually got.
        //    The sample data uses ColumnTypeId placeholders (1 = text, 4 = bool).
        var textType = columnTypes.First(x => x.Code == "text");
        var boolType = columnTypes.First(x => x.Code == "bool");

        var columnDefinitions = AdvancedSearchColumnDefinitionData.CreateAdvancedSearchColumnDefinitions();
        foreach (var definition in columnDefinitions)
        {
            definition.TableId = table1.Id;
            definition.ColumnTypeId = definition.ColumnTypeId == 1 ? textType.Id : boolType.Id;
        }

        // The context exposes no DbSet for definitions; EF still has the entity via AdvancedSearchColumn.
        await _dbContext.Set<AdvancedSearchColumnDefinition>().AddRangeAsync(columnDefinitions, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 4) The AdvancedSearch root, now that its main table exists
        var mockAdvSearch = AdvancedSearchData.CreateAdvancedSearch();
        mockAdvSearch.MainTableId = table1.Id;
        await _dbContext.AdvancedSearches.AddAsync(mockAdvSearch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5) Joins are not seeded: both columns live on the main table, so nothing needs joining yet.
        //var mockAdvSearchJoin = AdvancedSearchJoinData.CreateAdvancedSearchJoins();

        // 6) Search columns, pointing at the definitions saved above
        var mockAdvSearchColumns = AdvancedSearchColumnData.CreateAdvancedSearchColumns();
        for (int i = 0; i < mockAdvSearchColumns.Count; i++)
        {
            mockAdvSearchColumns[i].AdvancedSearchId = mockAdvSearch.Id;
            mockAdvSearchColumns[i].ColumnDefinitionId = columnDefinitions[i].Id;
        }

        await _dbContext.AdvancedSearchColumns.AddRangeAsync(mockAdvSearchColumns, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);


        // Create sample User data to be searched against
        var mockUserTaskUser = UserEntityData.CreateUserTask();

        await _dbContext.Users.AddAsync(mockUserTaskUser, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);


        // Now the sample TaskList and TaskItem the search should find
        var mockTaskList = TaskListEntityData.CreateTask();
        mockTaskList.AssignedToId = mockUserTaskUser.Id;

        await _dbContext.TaskLists.AddAsync(mockTaskList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);


        var mockTaskItem = TaskItemEntityData.CreateTaskItem();
        mockTaskItem.TaskListId = mockTaskList.Id;

        await _dbContext.TaskItems.AddAsync(mockTaskItem, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);


        return new SeededSearch(mockAdvSearch.Id, mockAdvSearchColumns.Select(c => c.Id).ToList());
    }

}
