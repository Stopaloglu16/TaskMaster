using Application.Common.Interfaces;
using Domain.Entities.Search;

namespace Application.Repositories
{
    public interface ISearchRepository : IRepository<SearchType, int>
    {
    }
}
