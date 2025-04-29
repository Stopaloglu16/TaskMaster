using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WebsiteApp.SeleniumTests
{
    public abstract class BaseTest : IDisposable
    {
        protected IWebDriver Driver;

        public BaseTest()
        {
            Driver = new ChromeDriver();
            Login();
        }

        private void Login()
        {
            Driver.Navigate().GoToUrl("https://localhost:7155");
            Driver.FindElement(By.Name("Username")).SendKeys("testuser");
            Driver.FindElement(By.Name("Password")).SendKeys("testpass");
            Driver.FindElement(By.LinkText("Login")).Click();
        }

        public void Dispose()
        {
            Driver.Quit();
        }
    }
}
