using Domain.Entities.SearchEntities;

namespace SharedTestDataLibrary.AdvancedSearchSample;

public class AdvancedSearchData
{
    public static AdvancedSearch CreateAdvancedSearch()
    {
        return new AdvancedSearch() { Name = "TaskList", IsDeleted = 0, MainTableId = 1  };
    }

}
