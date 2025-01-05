using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Application.Common.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApp.Config;

namespace WebApp.Services;

public class AuthService : IAuthService
{

    public HttpClient _httpClient { get; }
    //public AppSettings _appSettings { get; }
    public ApiSettingConfig _apiSettingConfig { get; }

    private readonly string _apiVersion = "v1.0";

    public AuthService(HttpClient httpClient, IOptions<ApiSettingConfig> apiSettingConfig)
    {
        _apiSettingConfig = apiSettingConfig.Value;

        httpClient.BaseAddress = new Uri(_apiSettingConfig.ApiAuthUrl);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");

        _httpClient = httpClient;
    }


    public async Task<UserLoginResponse> LoginAsync(UserLoginRequest loginRequest)
    {
        //loginRequest.Password = await EncryptDecrypt.EncryptAsyc(loginRequest.Password, true, _appSettings.KeyEncrypte);
        string serializedUser = JsonConvert.SerializeObject(loginRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/{_apiVersion}/Login/login");
        requestMessage.Content = new StringContent(serializedUser);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        var responseStatusCode = response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();


        var mysign = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);

        return mysign;
    }


    public async Task<UserLoginResponse> GetUserByAccessTokenAsync(TokenRefreshRequest tokenRefreshRequest)
    {
        string serializedRefreshRequest = JsonConvert.SerializeObject(tokenRefreshRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/{_apiVersion}/Login/refresh-token");
        requestMessage.Content = new StringContent(serializedRefreshRequest);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        var responseStatusCode = response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();

        var returnedUser = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);

        return await Task.FromResult(returnedUser);
    }


    public Task<CustomResult> RegisterUserAsync(RegisterUserRequest registerUserRequest)
    {
        throw new NotImplementedException();
    }
}
