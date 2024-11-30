﻿namespace WebApp.Services;

public interface IWebApiService<TRequest, TResponse>
{

    Task<List<TResponse>> GetAllDataAsync(string requestUri);

    Task<TResponse> GetDataByIdAsync(string requestUri);

    Task<TResponse> SaveAsync(string requestUri, TRequest obj);

    //Task<TResponse> SaveBulkAsync(string requestUri, List<TRequest> obj);

    Task<TResponse> UpdateAsync(string requestUri, int Id, TRequest obj);

    Task<TResponse> DeleteAsync(string requestUri);

}
