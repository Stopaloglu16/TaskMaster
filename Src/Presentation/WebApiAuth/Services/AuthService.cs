using Application.Aggregates.UserAuthAggregate;
using Application.Aggregates.UserAuthAggregate.Token;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceLayer.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiAuth.Models;

namespace WebApiAuth.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtsettings;
    private readonly IUserService _userloginservice;

    public AuthService(IOptions<JwtSettings> jwtsettings,
                       IUserService userService)
    {
        _jwtsettings = jwtsettings.Value;
        _userloginservice = userService;
    }


    public async Task<UserLoginResponse?> LoginAsync(UserTokenDto userTokenDto)
    {
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(userTokenDto.UserId);

        var LoginResponse = new UserLoginResponse();

        LoginResponse.RefreshToken = refreshToken;
        LoginResponse.AccessToken = GenerateAccessToken(userTokenDto.Username, userTokenDto.UserGuidId.ToString(), userTokenDto.Role);
        LoginResponse.UserName = userTokenDto.Username;

        return LoginResponse;
    }

    public async Task<UserLoginResponse?> RefreshTokensAsync(UserTokenDto userTokenDto)
    {
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(userTokenDto.UserId);

        var LoginResponse = new UserLoginResponse();

        LoginResponse.RefreshToken = refreshToken;
        LoginResponse.AccessToken = GenerateAccessToken(userTokenDto.Username, userTokenDto.UserGuidId.ToString(), userTokenDto.Role);
        LoginResponse.UserName = userTokenDto.Username;

        return LoginResponse;
    }



    private async Task<string> GenerateAndSaveRefreshTokenAsync(int userId)
    {
        var refreshToken = GenerateRefreshToken();

        await _userloginservice.UpdateRefreshTokenAsync(userId, refreshToken, DateTime.UtcNow.AddDays(7));

        return refreshToken;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string GenerateAccessToken(string username, string userGuidId, string userRole)
    {

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userGuidId),
                new Claim(ClaimTypes.Role, userRole)
            };

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtsettings.SecretKey));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var tokeOptions = new JwtSecurityToken(
            issuer: _jwtsettings.Issuer,
            audience: _jwtsettings.Audience,
            claims: claims, 
            notBefore:DateTime.Now,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: signinCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
    }


    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Ignore expiration for refresh validation
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtsettings.Issuer,
            ValidAudience = _jwtsettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtsettings.SecretKey))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        return principal;
    }

}
