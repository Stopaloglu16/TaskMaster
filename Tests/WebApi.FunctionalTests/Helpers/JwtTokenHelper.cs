using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApi.FunctionalTests.Helpers
{
    public static class JwtTokenHelper
    {
        public static string GenerateJwtToken(string secretKey, string issuer, string audience)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, "TestUser"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name, "MockUser"),
            new Claim(JwtRegisteredClaimNames.GivenName, "MockUser"),
            new Claim(JwtRegisteredClaimNames.NameId, "MockUserId"),
            new Claim(ClaimTypes.NameIdentifier, "MockUserId")
            };

            //mylist.Add(new Claim(ClaimTypes.Name, "TestUser"));
            //mylist.Add(new Claim(ClaimTypes.NameIdentifier, "MockUserId"));
            //mylist.Add(new Claim(ClaimTypes.GivenName, "MockUserName"));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);



            //var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);

            //var mylist = new List<Claim>();

            ////mylist.Add(new Claim(ClaimTypes.Name, userId));

            //mylist.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            //mylist.Add(new Claim(ClaimTypes.GivenName, userName));


            //mylist.Add(new Claim(ClaimTypes.Role, userType.ToString()));


            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(mylist),
            //    Expires = DateTime.UtcNow.AddDays(1),
            //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
            //    SecurityAlgorithms.HmacSha256Signature)
            //};


            //var token = tokenHandler.CreateToken(tokenDescriptor);
            //return tokenHandler.WriteToken(token);
        }
    }
}
