using Application.Common.Models;
using Blazored.LocalStorage;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApp.Config;

namespace WebApp.Services;

public class WebApiService<TRequest, TResponse> : IWebApiService<TRequest, TResponse>
{

    public HttpClient _httpClient { get; }
    public ApiSettingConfig _apiSettingConfig { get; }
    public ILocalStorageService _localStorageService { get; }

    public string Apitext { get; set; } = $"/api/v1.0/";

    public WebApiService(HttpClient httpClient,
                         IOptions<ApiSettingConfig> apiSettingConfig,
                         ILocalStorageService localStorageService)
    {
        _apiSettingConfig = apiSettingConfig.Value;
        _localStorageService = localStorageService;
        
        httpClient.BaseAddress = new Uri(_apiSettingConfig.ApiUrl);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");

        _httpClient = httpClient;
    }

    public async Task<PagingResponse<TResponse>> GetAllDataAsync(string requestUri)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, Apitext + requestUri);

        var token = await _localStorageService.GetItemAsync<string>("accessToken");
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            return await Task.FromResult(JsonConvert.DeserializeObject<PagingResponse<TResponse>>(responseBody));
        }
        else
            return null;
    }

    public async Task<TResponse> GetDataByIdAsync(string requestUri)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, Apitext + requestUri);

        var token = await _localStorageService.GetItemAsync<string>("accessToken");
        requestMessage.Headers.Authorization
            = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(requestMessage);

        var responseStatusCode = response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(JsonConvert.DeserializeObject<TResponse>(responseBody));
    }

    public async Task<TResponse> SaveAsync(string requestUri, TRequest obj)
    {
        string serializedUser = JsonConvert.SerializeObject(obj);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, Apitext + requestUri);

        var token = await _localStorageService.GetItemAsync<string>("accessToken");
        requestMessage.Headers.Authorization
            = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        requestMessage.Content = new StringContent(serializedUser);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        var responseStatusCode = response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(JsonConvert.DeserializeObject<TResponse>(responseBody));
    }

    public async Task<TResponse> SaveBulkAsync(string requestUri, List<TRequest> obj)
    {
        string serializedUser = JsonConvert.SerializeObject(obj);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, Apitext + requestUri);

        var token = await _localStorageService.GetItemAsync<string>("accessToken");
        requestMessage.Headers.Authorization
            = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        requestMessage.Content = new StringContent(serializedUser);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        var responseStatusCode = response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();

        var returnedObj = JsonConvert.DeserializeObject<TResponse>(responseBody);

        return await Task.FromResult(returnedObj);
    }

    public async Task<TResponse> UpdateAsync(string requestUri, int Id, TRequest obj)
    {
        
        string serializedUser = JsonConvert.SerializeObject(obj);

        var requestMessage = new HttpRequestMessage(HttpMethod.Put, Apitext + requestUri + Id);

        var token = await _localStorageService.GetItemAsync<string>("accessToken");
        requestMessage.Headers.Authorization
            = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        requestMessage.Content = new StringContent(serializedUser);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);


        var responseStatusCode = response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(JsonConvert.DeserializeObject<TResponse>(responseBody));
    }


    public async Task<TResponse> DeleteAsync(string requestUri)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, Apitext + requestUri);

        var token = await _localStorageService.GetItemAsync<string>("accessToken");
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(requestMessage);


        var responseStatusCode = response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(JsonConvert.DeserializeObject<TResponse>(responseBody));
    }


    
}
