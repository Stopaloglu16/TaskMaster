using Domain.Entities.SearchEntities;

namespace SharedTestDataLibrary.AdvancedSearchSample;

public class AdvancedSearchColumnDefinitionData
{
    public static List<AdvancedSearchColumnDefinition> CreateAdvancedSearchColumnDefinitions()
    {
        return new List<AdvancedSearchColumnDefinition>()
        {
            new AdvancedSearchColumnDefinition()
            {
                TableId = 1,
                ColumnName = "Title",
                DisplayName = "Title",
                ColumnTypeId = 1
            },
            new AdvancedSearchColumnDefinition()
            {
                TableId = 1,
                ColumnName = "IsCompleted",
                DisplayName = "Is Completed",
                ColumnTypeId = 4
            }
        };
    }
}


