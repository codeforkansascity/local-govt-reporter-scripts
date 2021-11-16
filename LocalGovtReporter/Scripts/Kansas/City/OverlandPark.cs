using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Kansas.City
{
    public class OverlandPark : IScript
    {
        public string AgencyName { get { return "City of Overland Park"; } }
        public string SiteURL { get { return "https://opkansas.civicweb.net/Portal/MeetingTypeList.aspx"; } }
        public async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();
            IWebDriver subPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl(SiteURL);
            Thread.Sleep(5000);

            HelperMethods.OverlandParkGetMeetings(mainPageDriver, subPageDriver, ".upcoming-meeting-list", meetingsList);
            HelperMethods.OverlandParkGetMeetings(mainPageDriver, subPageDriver, ".recent-meeting-list", meetingsList);


            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList, AgencyName);

            mainPageDriver.Quit();
            subPageDriver.Quit();
        }    
    }
}