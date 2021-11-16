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
        public string AgencyName { get { return "Kansas City, MO"; } }
        public string SiteURL { get { return "https://clerk.kcmo.gov/Calendar.aspx"; } }
        public async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl(SiteURL);

            HelperMethods.KCMOGetMeetings(mainPageDriver, ".outputRow", meetingsList);
            //HelperMethods.KCMOGetMeetings(mainPageDriver, ".altOutputRow", meetingsList);
            
            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList, AgencyName);

            mainPageDriver.Quit();
        }
    }
}