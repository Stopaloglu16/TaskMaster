using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.Script;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsiteApp.SeleniumTests.Utilities;

namespace WebsiteApp.SeleniumTests.TaskManager
{
    public class TaskManagerUploadTest : BaseTest
    {



        IWebElement BtnNewButton => _webDriver.FindElement(By.Id("CreateNewTaskButton"));
        IWebElement TxtTaskTitle => _webDriver.FindElement(By.Id("Title"));
        IWebElement TxtDueDate => _webDriver.FindElement(By.Id("DueDate"));
        IWebElement BtnSaveTask => _webDriver.FindElement(By.Id("NewTaskSaveBtn"));


        [Fact]
        public void CanViewProductList()
        {
            //Step 1: Navigate to the page
            //Arrange
            _webDriver.Navigate().GoToUrl($"{BaseUrl}/taskmanagerupload");

            Assert.Contains("taskmanagerupload", _webDriver.Url);

            string baseDirectory = AppContext.BaseDirectory;
            string relativePath = "../../../FileSamples/tasklist-sample.csv";
            //C: \Users\serta\source\repos\TaskMaster\Tests\WebsiteApp.SeleniumTests\FileSamples\tasklist - sample.csv

            string uploadFile = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));


            //Act
            IWebElement fileInput = _webDriver.FindElement(By.CssSelector("input[type=file]"));
            fileInput.SendKeys(uploadFile);
            _webDriver.FindElement(By.Id("file-submit")).Click();

            WebDriverWait wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.XPath("//*[@id=\"TaskmanagerTableId\"]/div[1]/table/tbody/tr[1]/td[1]"))); // or any key element

            //Assert
            var tableRows = _webDriver.FindElements(By.XPath("//*[@id=\"TaskmanagerTableId\"]/div[1]/table/tbody/tr[1]/td[1]"));
            Assert.True(tableRows.Count == 1, "table rows count");


            //Step 2: Create a new task
            //Arrange
            CustomMethods.Click(BtnNewButton);


            CustomMethods.EnterText(TxtTaskTitle, "Task Mock1");
            CustomMethods.EnterText(TxtDueDate, "2025-10-31");

            //Act
            CustomMethods.Click(BtnSaveTask);

            //Assert
            WebDriverWait wait1 = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.XPath("//*[@id=\"TaskmanagerTableId\"]/div[1]/table/tbody/tr[1]/td[1]"))); // or any key element


            var tableRows1 = _webDriver.FindElements(By.XPath("//*[@id=\"TaskmanagerTableId\"]/div[1]/table/tbody/tr[1]/td[1]"));
            Assert.True(tableRows1.Count == 1, "table rows count");



        }

    }
}
