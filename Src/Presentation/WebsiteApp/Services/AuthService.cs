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

    private readonly string _apiVersion = "api/v1.0";

    public AuthService(HttpClient httpClient, IOptions<ApiSettingConfig> apiSettingConfig)
    {
        _apiSettingConfig = apiSettingConfig.Value;

        httpClient.BaseAddress = new Uri(_apiSettingConfig.ApiAuthUrl);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");

        _httpClient = httpClient;
    }


    public async Task<CustomResult<UserLoginResponse>> LoginAsync(UserLoginRequest loginRequest)
    {
        try
        {
            //loginRequest.Password = await EncryptDecrypt.EncryptAsyc(loginRequest.Password, true, _appSettings.KeyEncrypte);
            string serializedUser = JsonConvert.SerializeObject(loginRequest);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiVersion}/Login/login");
            requestMessage.Content = new StringContent(serializedUser);

            requestMessage.Content.Headers.ContentType
                = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(requestMessage);

            var responseBody = await response.Content.ReadAsStringAsync();
            //UnauthorizedSystem.Threading.Tasks.Task`1[System.String]


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var mysign = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);
                return CustomResult<UserLoginResponse>.Success(mysign);
            }
            else
            {
                // responseBody is already the body; the old version re-read the stream, blocked on
                // .Result, and concatenated the HttpContent object itself, which is where the
                // "System.Net.Http.HttpConnectionResponseContent" tail on every error came from.
                return CustomResult<UserLoginResponse>.Failure(new CustomError(false,
                    responseBody.Trim('"')));
            }
        }
        catch (Exception ex)
        {
            return CustomResult<UserLoginResponse>.Failure(new CustomError(false, "Ex:" + ex.Message));
        }
    }


    public async Task<UserLoginResponse> GetUserByTokenAsync(RefreshTokenRequest tokenRefreshRequest)
    {
        string serializedRefreshRequest = JsonConvert.SerializeObject(tokenRefreshRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiVersion}/Login/refresh-token");
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

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiVersion}/registerusers");
        requestMessage.Content = new StringContent(serializedRefreshRequest);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return CustomResult.Success();

        var responseBody = await response.Content.ReadAsStringAsync();

        return CustomResult.Failure(responseBody);
    }

    public async Task<CustomResult> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
    {
        string serializedResetRequest = JsonConvert.SerializeObject(resetPasswordRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiVersion}/resetpassword");
        requestMessage.Content = new StringContent(serializedResetRequest);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return CustomResult.Success();

        var responseBody = await response.Content.ReadAsStringAsync();

        return CustomResult.Failure(responseBody);
    }

    public async Task<CustomResult> ForgotPasswordRequestAsync(Application.Aggregates.UserAuthAggregate.ForgotPasswordRequest forgotPasswordRequest)
    {
        string serializedRefreshRequest = JsonConvert.SerializeObject(forgotPasswordRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_apiVersion}/forgotpassword");
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
