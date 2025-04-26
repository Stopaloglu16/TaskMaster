using Bunit;
using Radzen.Blazor;
using WebsiteApp.Components.Pages;

namespace WebsiteApp.BUnitTests
{

    public class HomeTest : TestContext
    {
        [Fact(Skip = "asdsa")]
        public void Home_Valid_Test()
        {
            // Act
            var cut = RenderComponent<Home>();

            // Act: find and click the <button> element to increment
            // the counter in the <p> element
            var tableCaption = cut.Find("div#findmetest").TextContent;

            var cut1 = cut.FindComponent<RadzenCard>();

            var cut2 = cut.FindComponent<RadzenButton>();

            var ppp = cut2.Instance.Text;

            Assert.True(false);
            // Assert: first find the <p> element, then verify its content
            //cut.Find("p").MarkupMatches(@"<p role=""status"">Current count: 1</p>");
        }
    }
}
