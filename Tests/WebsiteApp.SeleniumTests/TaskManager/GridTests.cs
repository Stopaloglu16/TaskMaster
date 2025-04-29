using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteApp.SeleniumTests.TaskManager
{
    public class GridTests:BaseTest
    {

        [Fact]
        public void CanViewProductList()
        {
            Driver.Navigate().GoToUrl("https://localhost:7155/taskmanager");
            // assertions here
        }

    }
}
