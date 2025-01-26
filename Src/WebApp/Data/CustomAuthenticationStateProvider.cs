using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Azure.Core;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                identity = new ClaimsIdentity();
            }
            else
            {
                RefreshTokenRequest tokenRefreshRequest = new RefreshTokenRequest() { RefreshToken = refreshToken, AccessToken = accessToken };

                var user = await _authService.GetUserByAccessTokenAsync(tokenRefreshRequest);

                identity = new ClaimsIdentity(GetClaimsIdentity(user), "testAuthType");
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);

            return claimsPrincipal;
        }
        catch (Exception ex)
        {
            return default;
        }
    }

    public async Task MarkUserAsAuthenticated(UserLoginResponse user)
    {
        await _localStorageService.SetItemAsync("accessToken", user.AccessToken);
        await _localStorageService.SetItemAsync("refreshToken", user.RefreshToken);

        var identity = new ClaimsIdentity(GetClaimsIdentity(user), "testAuthType");

        var myuser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(myuser)));
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

        if (user != null )
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

                //claimsIdentity = new ClaimsIdentity(new[]
                //                {
                //                        new Claim(ClaimTypes.Name, user.UserName)
                //                    }, "apiauth_type");
            }
        }


        return new List<Claim>();
    }


    public async Task GetTokenExpieryDateTime()
    {
        var handler = new JwtSecurityTokenHandler();

        var accessToken = await _localStorageService.GetItemAsync<string>("accessToken");

        var jwtToken = handler.ReadJwtToken(accessToken);

        if (jwtToken.ValidTo < DateTime.Now.AddMinutes(-1))
        {
            var refreshToken = await _localStorageService.GetItemAsync<string>("refreshToken");
            await AuthenticateUser(accessToken, refreshToken);
        }
    }

    public IDictionary<string, string> GetClaimsFromAccessToken(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();

        if (handler.CanReadToken(accessToken))
        {
            var jwtToken = handler.ReadJwtToken(accessToken);

            // Extract claims
            // var claims = jwtToken.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);


            var mappedClaims = jwtToken.Claims.ToDictionary(
        claim => claim.Type switch
        {
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => "role",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => "name",
            _ => claim.Type // Keep other claims as-is
        },
        claim => claim.Value
    );


            return mappedClaims;
        }

        throw new ArgumentException("Invalid access token");
    }
}
