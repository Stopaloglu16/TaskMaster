using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace WebApp.Services;

public class AuthService : IAuthService
{

    public HttpClient _httpClient { get; }
    public AppSettings _appSettings { get; }

    public AuthService(HttpClient httpClient, IOptions<AppSettings> appSettings)
    {
        //_appSettings = appSettings.Value;

        //httpClient.BaseAddress = new Uri(_appSettings.ApiAddressForDatabase);
        httpClient.BaseAddress = new Uri("https://localhost:7132");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BlazorServer");

        _httpClient = httpClient;
    }


    public async Task<UserLoginResponse> GetUserByAccessTokenAsync(string accessToken)
    {
        throw new NotImplementedException();
    }

    public async Task<UserLoginResponse> LoginAsync(UserLoginRequest loginRequest)
    {
        //loginRequest.Password = await EncryptDecrypt.EncryptAsyc(loginRequest.Password, true, _appSettings.KeyEncrypte);
        string serializedUser = JsonConvert.SerializeObject(loginRequest);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1.0/Login/login");
        requestMessage.Content = new StringContent(serializedUser);

        requestMessage.Content.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.SendAsync(requestMessage);

        var responseStatusCode = response.StatusCode;
        var responseBody = await response.Content.ReadAsStringAsync();

        //CustomIdentityResult mysign = new CustomIdentityResult();

        var mysign = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);

        return mysign;
    }

    public Task<CustomResult> RegisterUserAsync(RegisterUserRequest registerUserRequest)
    {
        throw new NotImplementedException();
    }
}
