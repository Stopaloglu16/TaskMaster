using Application.Aggregates.SearchAggregate.Queries;
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
        token = JwtTokenHelper.GenerateJwtToken("adf8059594f8916b26kJ9TRNJqP#kKhneRjCDccJH44a4b8f0785f2aa805a2e933583376ea5e7d053fbc08c85e", "YourIssuer", "YourAudience");

        // Set JWT Token in the Authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    [Fact]
    public async Task CreateTaskItem_AdvancedSearch_ReturnSuccess()
    {

        // Arrange
        var userId = await ArrangeDb();

        var mockRequestDto = AdvancedSearchRequestData.CreateAdvancedSearchRequest();

        // Arrange
        mockRequestDto.SelectedColumnIds = new List<int> { 1, 2 };
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
        Assert.Equal(1, 0);
    }



    public async Task<string> ArrangeDb()
    {
        CancellationToken cancellationToken = new CancellationToken();

        try
        {
            // 1) Seed column types (and their operators) first so EF generates IDs we can reference
            var columnTypes = ColumnTypeData.CreateColumnTypes();
            await _dbContext.ColumnTypes.AddRangeAsync(columnTypes, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 2) Create and save the AdvancedSearch root entity to get its Id
            var mockAdvSearch = AdvancedSearchData.CreateAdvancedSearch();
            await _dbContext.AdvancedSearches.AddAsync(mockAdvSearch, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 3) Create tables and set AdvancedSearchId
            var table1 = AdvancedSearchTableData.CreateTaskListTable();
            //table1.ParentTableId  = mockAdvSearch.Id;
            var table2 = AdvancedSearchTableData.CreateTaskItemTable();
            //table2.AdvancedSearchId = mockAdvSearch.Id;
            await _dbContext.AdvancedSearchTables.AddRangeAsync(new[] { table1, table2 }, cancellationToken);

            // 4) Create joins and set AdvancedSearchId
            //var mockAdvSearchJoin = AdvancedSearchJoinData.CreateAdvancedSearchJoins();
            //mockAdvSearchJoin.ForEach(j => j.AdvancedSearchId = mockAdvSearch.Id);
            //await _dbContext.AdvancedSearchJoins.AddRangeAsync(mockAdvSearchJoin, cancellationToken);

            // 5) Create columns, map their ColumnTypeId to persisted ColumnType ids, and set AdvancedSearchId
            var mockAdvSearchColumns = AdvancedSearchColumnData.CreateAdvancedSearchColumns();
            foreach (var col in mockAdvSearchColumns)
            {
                col.AdvancedSearchId = mockAdvSearch.Id;

                // Sample mapping: the sample data used ColumnTypeId placeholders (1=text, 4=bool).
                // Map based on ColumnType.Code to the actual saved ids to avoid assuming DB-generated ids.
                if (col.ColumnDefinition.ColumnTypeId  == 1)
                {
                    var ct = columnTypes.FirstOrDefault(x => x.Code == "text");
                    if (ct != null) col.ColumnDefinition.ColumnTypeId = ct.Id;
                }
                else if (col.ColumnDefinition.ColumnTypeId == 2)
                {
                    var ct = columnTypes.FirstOrDefault(x => x.Code == "bool");
                    if (ct != null) col.ColumnDefinition.ColumnTypeId = ct.Id;
                }
                else
                {
                    // fallback: attach first ColumnType
                    col.ColumnDefinitionId = columnTypes.First().Id;
                }
            }
            await _dbContext.AdvancedSearchColumns.AddRangeAsync(mockAdvSearchColumns, cancellationToken);

            // 6) Persist all the newly added entities
            await _dbContext.SaveChangesAsync(cancellationToken);


            // Create sample User data to be searched against
            var mockUserTaskUser = UserEntityData.CreateUserTask();

            await _dbContext.Users.AddAsync(mockUserTaskUser, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);


            // 1) Now create sample TaskList and TaskItem data to be searched against
            var mockTaskList = TaskListEntityData.CreateTask();

            await _dbContext.TaskLists.AddAsync(mockTaskList, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);


            var mockTaskItem = TaskItemEntityData.CreateTaskItem();
            mockTaskItem.TaskListId = 1;

            await _dbContext.TaskItems.AddAsync(mockTaskItem, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);


            return "mockAspId";
        }
        catch (Exception ex)
        {
            return "fail";
        }

       
    }

}
