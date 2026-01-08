using Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Aggregates.TaskListAggregate.Commands.CreateUpdate
{
    public record CreateTaskListResponse
    {
        public int RowId { get; set; }
        public CustomError CustomError { get; set; } = CustomError.Success();

    }
}
