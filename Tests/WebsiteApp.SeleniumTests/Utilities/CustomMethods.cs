using OpenQA.Selenium;

namespace WebsiteApp.SeleniumTests.Utilities;

internal class CustomMethods
{
    public static void Click1(IWebDriver driver, By locator)
    {
        driver.FindElement(locator).Click();
    }

    public static void EnterText1(IWebDriver driver, By locator, string text)
    {
        driver.FindElement(locator).Clear();
        driver.FindElement(locator).SendKeys(text);
    }

    public static void Click(IWebElement locator)
    {
        locator.Click();
    }

    public static void Submit(IWebElement locator)
    {
        locator.Submit();
    }

    public static void EnterText(IWebElement locator, string text)
    {
        locator.Clear();
        locator.SendKeys(text);
    }
}
