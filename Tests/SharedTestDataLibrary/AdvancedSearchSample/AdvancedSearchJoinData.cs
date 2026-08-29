using Domain.Entities.SearchEntities;

namespace SharedTestDataLibrary.AdvancedSearchSample;

public class AdvancedSearchJoinData
{
    public static List<AdvancedSearchJoin> CreateAdvancedSearchJoins()
    {
        return new List<AdvancedSearchJoin>()
        {
            new AdvancedSearchJoin()
            {
                AdvancedSearchId = 2,
                FromTableAliasId = 1,
                ToTableAliasId = 2,
                JoinType = "INNER",
                JoinCondition = "tl.Id = ti.TaskListId"
            }
        };
    }

}
