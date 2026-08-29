using Domain.Entities;

namespace SharedTestDataLibrary.TaskDataSample;

public class TaskListEntityData
{

    public static TaskList CreateTask()
    {
        return new TaskList()
        {
            Title = "Mock Task List 1",
            DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
            IsCompleted = false,
            PriorityId = 1,
            AssignedToId = 1
        };
    }

    public static List<TaskList> CreateTaskLists(int count)
    {

        List<TaskList> taskListList = new List<TaskList>(count);

        for (int i = 0; i < count; i++)
        {
            taskListList.Add(new TaskList()
            {
                Title = "Mock Task List {i}",
                DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                IsCompleted = false,
                PriorityId = 1,
                AssignedToId = 1
            });
        }

        return taskListList;
    }
}
