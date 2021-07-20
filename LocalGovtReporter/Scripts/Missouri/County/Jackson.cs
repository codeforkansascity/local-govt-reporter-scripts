using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Missouri.County
{
    class Jackson : IScript
    {
        public async Task RunScriptAsync()
        {
            HelperMethods.MessageBuildingMeetingList("Jackson County, MO");

            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl("https://jacksonco.legistar.com/Calendar.aspx");

            HelperMethods.JacksonCountyGetMeetings(mainPageDriver, ".rgRow", meetingsList);
            HelperMethods.JacksonCountyGetMeetings(mainPageDriver, ".rgAltRow", meetingsList);

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);

            mainPageDriver.Quit();
        }
    }
}