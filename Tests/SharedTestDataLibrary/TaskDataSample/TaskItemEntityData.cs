using Domain.Entities;

namespace SharedTestDataLibrary.TaskDataSample;

public class TaskItemEntityData
{
    public static TaskItem CreateTaskItem()
    {
        return new TaskItem() { Title = "Mock Test Item 1", Description = "Mock Description" };
    }


    public static List<TaskItem> CreateTaskItemList(int count)
    {
        List<TaskItem> taskItems = new List<TaskItem>(count);

        for (int i = 0; i < count; i++)
        {
            taskItems.Add(new TaskItem() { Title = $"Mock Test Item {i + 1}", Description = $"Mock Description {i + 1}" });
        }

        return taskItems;
    }

}
