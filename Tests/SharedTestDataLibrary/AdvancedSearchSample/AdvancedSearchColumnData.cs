using Domain.Entities.SearchEntities;

namespace SharedTestDataLibrary.AdvancedSearchSample;

public class AdvancedSearchColumnData
{
    public static List<AdvancedSearchColumn> CreateAdvancedSearchColumns()
    {
        return new List<AdvancedSearchColumn>()
        {
            new AdvancedSearchColumn()
            {
                AdvancedSearchId = 1,
                ColumnDefinitionId = 1,
                IsSelectable = true,
                IsFilterable = true,
                IsSortable = true
            },
            new AdvancedSearchColumn()
            {
                AdvancedSearchId = 1,
                ColumnDefinitionId = 2,
                IsSelectable = true,
                IsFilterable = true,
                IsSortable = true
            }
        };
    }
}



