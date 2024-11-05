using Application.Aggregates.UserAuthAggregate;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace WebApiAuth.FunctionalTests.ApiEndPoints;

public class LoginTests : BaseIntegrationTest
{

    

    public LoginTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
     
    }

    [Fact]
    public async Task Create_ShouldCreateUser()
    {


        var myUU =  await _factory.GetAspNetUserByUserNameAsync(UserType.AdminUser.ToString());

        try
        {
            
            var uuLL = await _dbContext.Users.ToListAsync();
        }
        catch (Exception ex)
        {

            throw;
        }

        

        //Arrange
        var loginRequest = new LoginRequest() { Username = $"AdminUser@hotmail.co.uk", Password = "SuperStrongPassword+123" };

        //Act
        var responseApiLogin = await _httpClient.PostAsJsonAsync($"/api/v1.0/Login/login", loginRequest);


        //Assert
        Assert.True(System.Net.HttpStatusCode.OK == responseApiLogin.StatusCode, $"Login API {responseApiLogin.StatusCode}");

        var apiLoginResponse = await responseApiLogin.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(apiLoginResponse?.RefreshToken);
    }

}
