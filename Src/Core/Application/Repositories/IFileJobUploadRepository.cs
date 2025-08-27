using Application.Aggregates.TaskItemAggregate.Queries;
using Application.Common.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IFileJobUploadRepository : IRepository<FileJobUpload, int>
    {

        

    }
}
