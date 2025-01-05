using Application.Common.Models;

namespace WebApp.Services;

public interface IWebApiService<TRequest, TResponse>
{

    Task<PagingResponse<TResponse>> GetAllDataAsync(string requestUri, bool requiresAuth = false);

    Task<TResponse> GetDataByIdAsync(string requestUri, bool requiresAuth = false);

    Task<HttpResponseMessage> SaveAsync(string requestUri, TRequest obj, bool requiresAuth = false);

    //Task<TResponse> SaveBulkAsync(string requestUri, List<TRequest> obj);

    Task<TResponse> UpdateAsync(string requestUri, int Id, TRequest obj);

    Task<TResponse> DeleteAsync(string requestUri);

}
