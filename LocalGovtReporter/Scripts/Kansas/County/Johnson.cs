using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Kansas.County
{
    public class Johnson : IScript
    {
        public string AgencyName { get { return "Johnson County, KS"; } }
        public string SiteURL { get { return "https://boccmeetings.jocogov.org/onbaseagendaonline"; } }

        public async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl(SiteURL);
            //2021.7.20 - This should probably use the calendar for two months: https://www.jocogov.org/calendar-created/month?field_event_type_tid=4224


            HelperMethods.JohnsonCountyGetMeetings(mainPageDriver, "#meetings-list-upcoming", meetingsList);
            HelperMethods.JohnsonCountyGetMeetings(mainPageDriver, "#meeting-list-recent", meetingsList);

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList, AgencyName);

            mainPageDriver.Quit();
        }
    }
}