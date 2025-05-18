using Application.Common.Models;

namespace WebsiteApp.Services;

public interface IWebApiService<TRequest, TResponse>
{

    Task<PagingResponse<TResponse>> GetPagingDataAsync(string requestUri, bool requiresAuth = false);
    Task<List<TResponse>> GetAllDataAsync(string requestUri, bool requiresAuth = false);
    Task<TResponse> GetSingleDataAsync(string requestUri, bool requiresAuth = false);

    Task<TResponse> GetDataByIdAsync(string requestUri, bool requiresAuth = false);

    Task<HttpResponseMessage> SaveAsync(string requestUri, TRequest obj, bool requiresAuth = false);

    //Task<TResponse> SaveBulkAsync(string requestUri, List<TRequest> obj);

    Task<HttpResponseMessage> UpdateAsync(string requestUri, int Id, TRequest obj, bool requiresAuth = false);
    Task<HttpResponseMessage> PatchAsync(string requestUri, int Id, TRequest obj, bool requiresAuth = false);

    Task<HttpResponseMessage> DeleteAsync(string requestUri, int Id, bool requiresAuth = false);

}
