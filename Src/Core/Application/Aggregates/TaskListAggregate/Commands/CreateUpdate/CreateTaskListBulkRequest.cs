namespace Application.Aggregates.TaskListAggregate.Commands.CreateUpdate
{
    public record CreateTaskListBulkRequest(List<CreateTaskListRequest> createTaskListRequest, 
                                            string UserId, 
                                            string UserName);
    //{

    //    public List<CreateTaskListRequest> createTaskListRequest { get; set; }
    //    public string UserId { get; set; }
    //    public string UserName { get; set; }
    //}
}
