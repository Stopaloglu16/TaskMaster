using Application.Aggregates.UserAuthAggregate;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

using ServiceLayer.Users;
using System.Security.Claims;
using System.Text.Json;
using WebApp.Services;

namespace WebApp.Data;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{

    public ILocalStorageService _localStorageService { get; }
    public IAuthService _authService { get; set; }
    private readonly HttpClient _httpClient;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorageService,
                                             IAuthService authService,
                                             HttpClient httpClient)
    {
        _localStorageService = localStorageService;
        _authService = authService;
        _httpClient = httpClient;
    }

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var accessToken = await _localStorageService.GetItemAsync<string>("accessToken");

            ClaimsIdentity identity;

            if (accessToken != null && accessToken != string.Empty)
            {
                var user = await _authService.GetUserByAccessTokenAsync(accessToken);

                identity = GetClaimsIdentity(user);
            }
            else
            {
                identity = new ClaimsIdentity();
            }


            var claimsPrincipal = new ClaimsPrincipal(identity);

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch (Exception ex)
        {
            throw;
        }

    }

    public void MarkUserAsAuthenticated(UserLoginResponse user)
    {

        _localStorageService.SetItemAsync("accessToken", user.AccessToken);
        _localStorageService.SetItemAsync("refreshToken", user.RefreshToken);

        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, "test")
                };

        var identity = new ClaimsIdentity(claims, "testAuthType");
        
        var myuser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(myuser)));

    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorageService.RemoveItemAsync("refreshToken");
        await _localStorageService.RemoveItemAsync("accessToken");

        
        var identity = new ClaimsIdentity();

        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }


    private ClaimsIdentity GetClaimsIdentity(UserLoginResponse user)
    {
        var claimsIdentity = new ClaimsIdentity();

        if (user.UserEmail != null)
        {
            claimsIdentity = new ClaimsIdentity(new[]
                            {
                                    new Claim(ClaimTypes.Name, user.UserName)
                                    //new Claim(ClaimTypes.Role, "Add art"),
                                    //new Claim(ClaimTypes.Role, "Add movie"),
                                    //new Claim("IsUserEmployedBefore1990", IsUserEmployedBefore1990(user))
                                }, "apiauth_type");
        }

        //if (user.myRoles != null)
        //{
        //    foreach (Role role in user.myRoles)
        //    {
        //        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.RoleName));
        //    }
        //}

        return claimsIdentity;
    }

}
