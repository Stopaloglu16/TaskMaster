namespace Application.Aggregates.DashboardAggregate;

public record TopWidgetDto
{
    //Due date 
    public List<TaskListPercentage> taskListPercentages { get; set; } = new();
    public List<TaskItemPercentage> taskItemPercentages { get; set; } = new();
}


public record TaskListPercentage
{
    public string DueDate { get; set; }
    public int OpenTasks { get; set; }
    public int TotalTasks { get; set; }
}

public record TaskItemPercentage
{
    public string DueDateTaskItem { get; set; }
    public int OpenTaskItems { get; set; }
}
