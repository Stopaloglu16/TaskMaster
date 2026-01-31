using ServiceLayer.TaskPriorities;

namespace WebApi.Apis
{
    public static class TaskPriorityApi
    {

        public static RouteGroupBuilder TaskPriorityApiV1(this RouteGroupBuilder group)
        {

            // Route for query task lists
            group.MapGet("/taskpriorityselectlist", GetSelectList)
                 .WithSummary("Get select lists")
                 .WithDescription("Returns select lists.");

            return group;

        }

        public static async Task<IResult> GetSelectList(ITaskPriorityService taskPriorityService,
                                                                          CancellationToken cancellationToken)
        {
            var taskList = await taskPriorityService.GetSelectList();

            return TypedResults.Ok(taskList);
        }


    }
}
