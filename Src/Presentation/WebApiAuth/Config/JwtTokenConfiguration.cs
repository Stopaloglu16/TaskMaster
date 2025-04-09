using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApiAuth.Models;

namespace WebApiAuth.Config;

public static class JwtTokenConfiguration
{

    public static void AddJtwToken(this IServiceCollection services, JwtSettings jwtSettings)
    {
        //Add JWT configuration
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = true;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        var securityScheme = new OpenApiSecurityScheme()
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        var securityReq = new OpenApiSecurityRequirement
        {
            { securityScheme, new[] { "Bearer" } }
        };

        var contact = new OpenApiContact()
        {
            Name = "Sertac",
            Email = "test@hotmail.co.uk",
            Url = new Uri("https://sites.google.com/site/sertactopaloglu")
        };

        var info = new OpenApiInfo()
        {
            Version = "v1",
            Title = "Task Master Auth API",
            Description = "Task Master Auth API",
            TermsOfService = new Uri("https://sites.google.com/site/sertactopaloglu"),
            Contact = contact
        };

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", info);
            o.AddSecurityDefinition("Bearer", securityScheme);
            o.AddSecurityRequirement(securityReq);
        });


    }

}
