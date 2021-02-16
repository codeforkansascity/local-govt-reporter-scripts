using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Methods;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Missouri.County
{
    class Jackson : IScript
    {
        public async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            mainPageDriver.Navigate().GoToUrl("https://jacksonco.legistar.com/Calendar.aspx");

            await HelperMethods.ReadTableAsync(mainPageDriver);

            mainPageDriver.Quit();
        }
    }
}