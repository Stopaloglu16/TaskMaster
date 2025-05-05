using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebsiteApp.Services;

namespace WebsiteApp.Data;


public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{

    public ILocalStorageService _localStorageService { get; }

    //Another option for localstorage
    //public ProtectedLocalStorage _protectedLocalStorage { get; }

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
            var refreshToken = await _localStorageService.GetItemAsync<string>("refreshToken");

            var claimsPrincipal = await AuthenticateUser(accessToken, refreshToken);

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    public async Task<ClaimsPrincipal> AuthenticateUser(string accessToken, string refreshToken)
    {
        ClaimsIdentity identity;

        try
        {
            if (String.IsNullOrEmpty(accessToken) && String.IsNullOrEmpty(refreshToken))
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var tery = await IsAccessTokenExpired(accessToken);

            if (tery)
            {
                Console.WriteLine("access expier");

                RefreshTokenRequest tokenRefreshRequest = new RefreshTokenRequest() { RefreshToken = refreshToken, AccessToken = accessToken };

                var user = await _authService.GetUserByTokenAsync(tokenRefreshRequest);

                if (user == null)
                {
                    await MarkUserAsLoggedOut(); // No valid refresh token, log out
                    return new ClaimsPrincipal(new ClaimsIdentity());
                }


                await _localStorageService.SetItemAsync("accessToken", user.AccessToken);

                identity = new ClaimsIdentity(ParseJwtClaims(user.AccessToken), "testAuthType");
            }
            else
            {
                identity = new ClaimsIdentity(ParseJwtClaims(accessToken), "testAuthType");
            }

            return new ClaimsPrincipal(identity);

        }
        catch (Exception ex)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }

    public async Task MarkUserAsAuthenticated(UserLoginResponse user)
    {
        try
        {
            //await _protectedLocalStorage.SetAsync("accessToken", user.AccessToken);
            //await _protectedLocalStorage.SetAsync("refreshToken", user.RefreshToken);

            //var tempToken = await _protectedLocalStorage.GetAsync<string>("accessToken");

            await _localStorageService.SetItemAsync("accessToken", user.AccessToken);
            await _localStorageService.SetItemAsync("refreshToken", user.RefreshToken);

            var identity = new ClaimsIdentity(GetClaimsIdentity(user), "testAuthType");

            var myuser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(myuser)));
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);    
        }
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorageService.RemoveItemAsync("refreshToken");
        await _localStorageService.RemoveItemAsync("accessToken");

        var user = new ClaimsPrincipal(new ClaimsIdentity());

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }


    private IEnumerable<Claim> GetClaimsIdentity(UserLoginResponse user)
    {
        var claimsIdentity = new ClaimsIdentity();

        if (user != null)
        {
            if (!String.IsNullOrEmpty(user.UserName))
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(user.AccessToken))
                {
                    var jwtToken = handler.ReadJwtToken(user.AccessToken);
                    //jwtToken.ValidTo
                    return jwtToken.Claims.ToList();
                }
            }
        }

        return new List<Claim>();
    }


    public async Task GetTokenExpiery()
    {
        var accessToken = await _localStorageService.GetItemAsync<string>("accessToken");

        var IsExpired = await IsAccessTokenExpired(accessToken);


        if (IsExpired)
        {
            var refreshToken = await _localStorageService.GetItemAsync<string>("refreshToken");
            await AuthenticateUser(accessToken, refreshToken);
        }
    }

    private IEnumerable<Claim> ParseJwtClaims(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadToken(jwt) as JwtSecurityToken;
        return token?.Claims ?? Enumerable.Empty<Claim>();
    }

    private async Task<bool> IsAccessTokenExpired(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;
            if (jsonToken?.ValidTo != null)
            {
                return jsonToken.ValidTo <= DateTime.UtcNow;
            }
            return true; // Treat as expired if ValidTo is null
        }
        catch (Exception ex)
        {
            return true; // Treat as expired in case of errors to be safe.
        }
    }

}
