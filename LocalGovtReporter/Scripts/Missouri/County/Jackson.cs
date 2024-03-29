﻿using LocalGovtReporter.Interfaces;
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
        public string AgencyName { get { return "Jackson County, MO"; } }
        public string SiteURL { get { return "https://jacksonco.legistar.com/Calendar.aspx"; } }
        public async Task<int> RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl(SiteURL);

            HelperMethods.JacksonCountyGetMeetings(mainPageDriver, ".rgRow", meetingsList);
            HelperMethods.JacksonCountyGetMeetings(mainPageDriver, ".rgAltRow", meetingsList);

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList, AgencyName);

            mainPageDriver.Quit();
            return meetingsList.Count;
        }
    }
}