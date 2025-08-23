using Application.Aggregates.TaskListAggregate.Commands.CreateUpdate;
using ServiceLayer.TaskLists;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Models;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using Application.Common.Interfaces;

namespace WebApi.Services
{
    public class BackgroundService
    {

        private readonly ITaskListService _taskListService;
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundService(ITaskListService taskListService, IServiceScopeFactory scopeFactory)
        {
            _taskListService = taskListService;
            _scopeFactory = scopeFactory;
        }

        [TickerFunction(functionName: nameof(ProcessBulkTaskList))]
        public async Task ProcessBulkTaskList(TickerFunctionContext<CreateTaskListBulkRequest> createTaskListBulkRequest, CancellationToken cancellationToken)
        {
            await _taskListService.CreateTaskListBulkAsync(createTaskListBulkRequest.Request, cancellationToken);

        }

    }
}
