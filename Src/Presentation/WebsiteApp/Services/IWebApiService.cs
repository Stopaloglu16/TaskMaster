using Application.Common.Models;

namespace WebsiteApp.Services;

public interface IWebApiService<TRequest, TResponse>
{

    Task<PagingResponse<TResponse>> GetPagingDataAsync(string requestUri, CancellationToken cancellationToken = default, bool requiresAuth = false);
    Task<List<TResponse>> GetAllDataAsync(string requestUri, CancellationToken cancellationToken = default, bool requiresAuth = false);
    Task<TResponse> GetSingleDataAsync(string requestUri, bool requiresAuth = false);

    Task<TResponse> GetDataByIdAsync(string requestUri, bool requiresAuth = false);

    Task<HttpResponseMessage> SaveAsync(string requestUri, TRequest obj, bool requiresAuth = false);

    Task<HttpResponseMessage> SaveBulkAsync(string requestUri, List<TRequest> obj, CancellationToken cancellationToken = default, bool requiresAuth = false);
    Task<HttpResponseMessage> SaveBulkRabbitMqAsync(string requestUri, List<TRequest> obj, CancellationToken cancellationToken = default, bool requiresAuth = false);

    Task<HttpResponseMessage> SaveBulkTickerQAsync(string requestUri, List<TRequest> obj, CancellationToken cancellationToken = default, bool requiresAuth = false);

    Task<HttpResponseMessage> SaveBulkFileJobAsync(string requestUri, int FileJobId, List<TRequest> obj, CancellationToken cancellationToken = default, bool requiresAuth = false);

    Task<HttpResponseMessage> UpdateAsync(string requestUri, int Id, TRequest obj, bool requiresAuth = false);
    Task<HttpResponseMessage> PatchAsync(string requestUri, int Id, TRequest obj, bool requiresAuth = false);
    Task<HttpResponseMessage> PatchAsync(string requestUri, int Id, bool requiresAuth = false);
    Task<HttpResponseMessage> DeleteAsync(string requestUri, int Id, bool requiresAuth = false);

}
