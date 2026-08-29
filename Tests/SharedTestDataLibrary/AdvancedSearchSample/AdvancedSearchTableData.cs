using Domain.Entities.SearchEntities;

namespace SharedTestDataLibrary.AdvancedSearchSample;

public class AdvancedSearchTableData
{
    public static AdvancedSearchTable CreateTaskListTable()
    {
        return new AdvancedSearchTable()
        {
            
            TableName = "dbo.TaskLists",
            TableAlias = "tl",
            ParentTableId = 0,
            ForeignKeyColumn = "",
            ParentKeyColumn = "",
            IsDeleted = 0
        };
    }

    public static AdvancedSearchTable CreateTaskItemTable()
    {
        return new AdvancedSearchTable()
        {
            TableName = "dbo.TaskItems",
            TableAlias = "ti",
            ParentTableId = 1,
            ForeignKeyColumn = "Id",
            ParentKeyColumn = "TaskListId",
            IsDeleted = 0
        };
    }
}
