using Application.Repositories;
using Domain.Entities.Search;
using Infrastructure.Data;

namespace Infrastructure.Repositories.SearchRepos
{
    public class SearchRepository : EfCoreRepository<SearchType, int>, ISearchRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public SearchRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }






    }
}
