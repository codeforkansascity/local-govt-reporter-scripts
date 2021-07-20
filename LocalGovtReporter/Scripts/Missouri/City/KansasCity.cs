using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Missouri.City
{
    public class KansasCity : IScript
    {
        public async Task RunScriptAsync()
        {
            HelperMethods.MessageBuildingMeetingList("Kansas City, KS");

            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl("http://cityclerk.kcmo.org/liveweb/Meetings/HistoricalMeetings.aspx");

            HelperMethods.KCMOGetMeetings(mainPageDriver, ".outputRow", meetingsList);
            HelperMethods.KCMOGetMeetings(mainPageDriver, ".altOutputRow", meetingsList);
            
            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);

            mainPageDriver.Quit();
        }
    }
}