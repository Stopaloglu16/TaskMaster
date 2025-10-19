using Microsoft.Playwright;

namespace WebsiteApp.PlaywrightTests;

public class LoginTests : BasePlaywrightTests
{
    public LoginTests(AspireManager aspireManager) : base(aspireManager) { }

    [Fact]
    public async Task TestWebAppHomePage()
    {

        await ConfigureAsync<Projects.TaskMaster_AppHost>();

        await InteractWithPageAsync("WebsiteApp", async page =>
        {
            await page.GotoAsync("/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 60_000 // increase timeout if needed
            });

            // Extra guard: ensure load state is reached
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Optionally, wait for a specific UI element instead of relying only on load state:
            // await page.WaitForSelectorAsync("form#login", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 30_000 });

            var title = await page.TitleAsync();
            Assert.Equal("Login", title);

        });
    }
}