using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Kansas.County
{
    public class Wyandotte : IScript
    {
        public async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl("https://wycokck.civicclerk.com/web/home.aspx");
            Thread.Sleep(5000);

            HelperMethods.WyandotteCountyGetMeetings(mainPageDriver, "#aspxroundpanelCurrent", meetingsList);
            HelperMethods.WyandotteCountyGetMeetings(mainPageDriver, "#aspxroundpanelRecent2", meetingsList);

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);

            mainPageDriver.Quit();
        }
    }
}