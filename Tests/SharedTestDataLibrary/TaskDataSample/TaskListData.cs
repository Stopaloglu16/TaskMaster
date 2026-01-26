using Application.Aggregates.TaskItemAggregate.Commands.CreateUpdate;
using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using Domain.Enums;
using System.Collections.Generic;

namespace SharedTestDataLibrary.TaskDataSample;

public class TaskListData
{
    public static TaskListFormRequest CreateCreateTaskListRequestEmpty()
    {
        return new TaskListFormRequest() { Title = string.Empty, DueDate = DateOnly.FromDateTime(DateTime.Now), PriorityId = 1 };
    }

    public static TaskListFormRequest CreateCreateTaskListRequestValid()
    {
        return CreateCreateTaskListRequestEmpty() with { Title = "MockTitle" };
    }

    public static TaskListFormRequest CreateUpdateTaskListRequestEmpty()
    {
        return new TaskListFormRequest() { Id = 1, Title = string.Empty, DueDate = DateOnly.FromDateTime(DateTime.Now), PriorityId = 1 };
    }

    public static CreateTaskListRequest CreateTaskListRequestEmpty()
    {
        return new CreateTaskListRequest()
        {
            Title = string.Empty,
            DueDate = DateOnly.FromDateTime(DateTime.Now),
            AssignedTo = null,
            PriorityId = 1
        };
    }

    /// <summary>
    /// Create task list, task item by count
    /// </summary>
    /// <param name="taskCount"></param>
    /// <returns></returns>
    public static IEnumerable<CreateTaskListRequest> CreateTaskListRequestEmpty(int[] taskCount, string taskUser)
    {
        List<CreateTaskListRequest> taskListRequestList = new List<CreateTaskListRequest>();

        int taskListCount = taskCount[0];
        Random random = new Random();

        for (int i = 0; i < taskListCount; i++)
        {
            var taskListRequest = new CreateTaskListRequest()
            {
                Title = $"MockTitle{i + 1}",
                DueDate = DateOnly.FromDateTime(DateTime.Now),
                AssignedTo = taskUser,
                createTaskItemRequests = new List<CreateTaskItemRequest>(),
                PriorityId = 1
            };

            int taskItemCount = random.Next(1, taskCount[1]);

            for (int t = 0; t < taskItemCount; t++)
            {
                taskListRequest.createTaskItemRequests.Add(new CreateTaskItemRequest() { 
                    Title = $"MockTaskItem{t + 1}" ,
                    Description = $"MockTaskItemDescription{t + 1}",
                    RowId = t + 1
                });
            }

            taskListRequestList.Add(taskListRequest);
        }

        return taskListRequestList;
    }

}