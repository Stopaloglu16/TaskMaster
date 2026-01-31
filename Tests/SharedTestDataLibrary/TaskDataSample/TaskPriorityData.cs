using Domain.Entities;

namespace SharedTestDataLibrary.TaskDataSample;

public class TaskPriorityData
{
    public static List<TaskPriority> GetTaskPriotiryList()
    {

        List<TaskPriority> taskPriorities = new List<TaskPriority>() {

            new TaskPriority { Name = "Low", Color = "secondary", SortOrder = 1, IsDefault = false },
            new TaskPriority { Name = "Medium", Color = "info", SortOrder = 2 , IsDefault = false},
            new TaskPriority { Name = "High", Color = "warning", SortOrder = 3, IsDefault = false },
            new TaskPriority { Name = "Critical", Color = "danger", SortOrder = 4, IsDefault = false }
        };

        return taskPriorities;
    }
}
