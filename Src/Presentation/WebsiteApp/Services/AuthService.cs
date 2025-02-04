using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Application.Common.Models;
using Blazored.LocalStorage;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebsiteApp.Config;

namespace WebsiteApp.Services;

public class AuthService : IAuthService
{

    public HttpClient _httpClient { get; }
    //public AppSettings _appSettings { get; }
    public ApiSettingConfig _apiSettingConfig { get; }
    public ILocalStorageService _localStorageService { get; }

    private readonly string _apiVersion = "v1.0";

    public AuthService(HttpClient httpClient, IOptions<ApiSettingConfig> apiSettingConfig)
    {
        _apiSettingConfig = apiSettingConfig.Value;

        httpClient.BaseAddress = new Uri(_apiSettingConfig.ApiAuthUrl);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");

        _httpClient = httpClient;
    }


    public async Task<CustomResult<UserLoginResponse>> LoginAsync(UserLoginRequest loginRequest)
    {
        //loginRequest.Password = await EncryptDecrypt.EncryptAsyc(loginRequest.Password, true, _appSettings.KeyEncrypte);
        string serializedUser = JsonConvert.SerializeObject(loginRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/{_apiVersion}/Login/login");
        requestMessage.Content = new StringContent(serializedUser);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseBody = await response.Content.ReadAsStringAsync();


            var mysign = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);

            return CustomResult<UserLoginResponse>.Success(mysign);
        }
        else
        {
            var rtnMessage = await response.Content.ReadAsStringAsync();
            return CustomResult<UserLoginResponse>.Failure(new CustomError(false, rtnMessage));
        }
    }


    public async Task<UserLoginResponse> GetUserByTokenAsync(RefreshTokenRequest tokenRefreshRequest)
    {
        string serializedRefreshRequest = JsonConvert.SerializeObject(tokenRefreshRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/{_apiVersion}/Login/refresh-token");
        requestMessage.Content = new StringContent(serializedRefreshRequest);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var returnedUser = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);

            return await Task.FromResult(returnedUser);
        }
        else
        {
            // TODO need invalid token issue, Changed access token 5 mins
            var responseBody = await response.Content.ReadAsStringAsync();
            return null; // await Task.FromResult(responseBody);
        }
    }


    public async Task<CustomResult> RegisterUserAsync(RegisterUserRequest registerUserRequest)
    {
        string serializedRefreshRequest = JsonConvert.SerializeObject(registerUserRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"/api/{_apiVersion}/registerusers");
        requestMessage.Content = new StringContent(serializedRefreshRequest);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return CustomResult.Success();

        var responseBody = await response.Content.ReadAsStringAsync();

        return CustomResult.Failure(responseBody);
    }



}
