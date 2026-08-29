using Application.Aggregates.SearchAggregate.Queries;

namespace SharedTestDataLibrary.AdvancedSearchSample;

public class AdvancedSearchRequestData
{

    public static AdvancedSearchRequestDto CreateAdvancedSearchRequest()
    {
        return new AdvancedSearchRequestDto() { AdvancedSearchId = 1, Page = 1, PageSize = 10 };
    }

}



//public int AdvancedSearchId { get; set; }

//  public List<int> SelectedColumnIds { get; set; } = new();

//  public FilterGroupDto? Filters { get; set; }

//  public int Page { get; set; } = 1;
//  public int PageSize { get; set; } = 50;