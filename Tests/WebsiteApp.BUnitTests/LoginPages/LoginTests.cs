using Blazored.LocalStorage;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Radzen.Blazor;
using System.Security.Claims;
using WebsiteApp.Components.Pages.LoginPages;
using WebsiteApp.Config;
using WebsiteApp.Services;

namespace WebsiteApp.BUnitTests.LoginPages;

public class LoginTests : TestContext
{

    /// <summary>
    /// https://github.com/radzenhq/radzen-blazor/blob/master/Radzen.Blazor.Tests/LoginTests.cs
    /// </summary>

    [Fact]
    public void Login_Raises_LoginEvent()
    {
        using var ctx = new TestContext();

        var component = ctx.RenderComponent<RadzenLogin>();

        var clicked = false;

        component.SetParametersAndRender(parameters => {
            parameters.Add(p => p.Username, "user");
            parameters.Add(p => p.Password, "pwd");
            parameters.Add(p => p.Login, args => { clicked = true; });
        });

        component.Find("button").Click();

        Assert.True(clicked);
    }

    [Fact]
    public void LoginPage_Valid_Test()
    {
        // Arrange: Register required services
        var fakeNavigationManager = Services.GetRequiredService<FakeNavigationManager>();


        // Arrange: Register required services
        Services.AddSingleton<IAuthService, AuthService>();

        // Use a mocked HttpClient for better test isolation
        var mockHttpClient = new HttpClient(new MockHttpMessageHandler());
        Services.AddSingleton(mockHttpClient);

        Services.AddSingleton<IOptions<ApiSettingConfig>>(Options.Create(new ApiSettingConfig
        {
            ApiAuthUrl = "https://example.com",
            ApiUrl = "https://example.com"
        }));
        Services.AddBlazoredLocalStorage();

        // Mock AuthenticationState
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "testuser@example.com")
        }, "TestAuthentication"));

        var authState = new AuthenticationState(claimsPrincipal);
        var authenticationStateTask = Task.FromResult(authState);

        // Register the CascadingParameter
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthenticationStateProvider(authenticationStateTask));

        // Render the component
        var cut = RenderComponent<Login>(parameters => parameters
            .AddCascadingValue(authenticationStateTask)
        );

        // Act: Simulate user interaction
        var loginButton = cut.Find("button");
        Assert.NotNull(loginButton); // Ensure the button exists before interacting
        loginButton.Click();

        // Assert: Verify the alert visibility and content
        Assert.False(cut.Instance.IsVisibleAlert, "The alert should be visible after clicking the button.");

        Assert.Equal("home", fakeNavigationManager.Uri);
    }

    [Fact]
    public void LoginPage_InValid_Test()
    {
        // Arrange: Register required services
        Services.AddSingleton<IAuthService, AuthService>();

        // Use a mocked HttpClient for better test isolation
        var mockHttpClient = new HttpClient(new MockHttpMessageHandler(false));
        Services.AddSingleton(mockHttpClient);

        Services.AddSingleton<IOptions<ApiSettingConfig>>(Options.Create(new ApiSettingConfig
        {
            ApiAuthUrl = "https://example.com",
            ApiUrl = "https://example.com"
        }));
        Services.AddBlazoredLocalStorage();

        // Mock AuthenticationState
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "testuser@example.com")
        }, "TestAuthentication"));

        var authState = new AuthenticationState(claimsPrincipal);
        var authenticationStateTask = Task.FromResult(authState);

        // Register the CascadingParameter
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthenticationStateProvider(authenticationStateTask));

        // Render the component
        var cut = RenderComponent<Login>(parameters => parameters
            .AddCascadingValue(authenticationStateTask)
        );

        // Act: Simulate user interaction
        var loginButton = cut.Find("button");
        Assert.NotNull(loginButton); // Ensure the button exists before interacting
        loginButton.Click();

        // Assert: Verify the alert visibility and content
        Assert.True(cut.Instance.IsVisibleAlert, "The alert should be visible after clicking the button.");

        var alertt =  cut.FindComponent<RadzenAlert>();

        var alertElement = cut.Find("#LoginAlert");

        Assert.NotNull(alertElement);
        Assert.Contains("Username or password not correct", alertElement.TextContent);

    }
}

// Helper class for mocking AuthenticationStateProvider
public class TestAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly Task<AuthenticationState> _authenticationStateTask;

    public TestAuthenticationStateProvider(Task<AuthenticationState> authenticationStateTask)
    {
        _authenticationStateTask = authenticationStateTask;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return _authenticationStateTask;
    }
}


// Mock HttpMessageHandler for HttpClient
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly bool _shouldSucceed;

    public MockHttpMessageHandler(bool shouldSucceed = true)
    {
        _shouldSucceed = shouldSucceed;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_shouldSucceed)
        {
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{ \"accessToken\": \"adsqwewq\", \"refreshToken\": \"string\", \"userName\": \"User1\" }")
            });
        }
        else
        {
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"Username or password not correct\"}")
            });
        }
    }
}