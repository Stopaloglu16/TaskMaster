using Bunit;
using WebsiteApp.Components.Pages;

namespace WebsiteApp.BUnitTests;

public class CounterTest:TestContext
{
    [Fact]
    public void Counter_Valid_Test()
    {
        // Act
        var cut = RenderComponent<Counter>();

        // Act: find and click the <button> element to increment
        // the counter in the <p> element
        cut.Find("button").Click();

        // Assert: first find the <p> element, then verify its content
        cut.Find("p").MarkupMatches(@"<p role=""status"">Current count: 1</p>");
    }
}
