using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebsiteApp.SeleniumTests.Utilities;

namespace WebsiteApp.SeleniumTests
{
    public class BaseTest : IDisposable
    {
        protected IWebDriver _webDriver;
        protected string BaseUrl { get; private set; }


        public BaseTest()
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            BaseUrl = configuration["TestSettings:BaseUrl"];

            _webDriver = new ChromeDriver();

            Login();
        }


        IWebElement TxtUser => _webDriver.FindElement(By.Name("Username"));
        IWebElement TxtPassword => _webDriver.FindElement(By.Name("Password"));
        IWebElement BtnLogin => _webDriver.FindElement(By.XPath("//button[.//span[text()='Login']]"));

        public void Login()
        {
            _webDriver.Navigate().GoToUrl(BaseUrl);

            CustomMethods.EnterText(TxtUser, "taskmaster@hotmail.co.uk");
            CustomMethods.EnterText(TxtPassword, "SuperStrongPassword+123");
            CustomMethods.Click(BtnLogin);

            WebDriverWait wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("home")); 

            //_webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public void Dispose()
        {
            _webDriver.Quit();
            
        }
    }
}
