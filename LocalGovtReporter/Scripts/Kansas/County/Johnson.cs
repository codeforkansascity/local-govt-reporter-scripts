using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Kansas.County
{
    public static class Johnson
    {
        public static async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl("https://boccmeetings.jocogov.org/onbaseagendaonline");

            IWebElement upcomingMeetings = mainPageDriver.FindElement(By.CssSelector("#meetings-list-upcoming"));

            var upcomingMeetingsOutputRows = upcomingMeetings.FindElements(By.CssSelector(".meeting-row"));

            foreach (var outputRow in upcomingMeetingsOutputRows)
            {
                string meetingType = outputRow.FindElements(By.TagName("td"))[0].Text.Trim();
                string meetingDateRaw = outputRow.FindElements(By.TagName("td"))[2].Text.Trim();
                string meetingDate = DateTime.ParseExact(meetingDateRaw.Split(" ")[0], "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd"); ;
                string meetingTime = meetingDateRaw.Split(" ")[1];
                string agendaURL = string.Empty;
                try
                {
                    agendaURL = outputRow.FindElements(By.TagName("td"))[5].FindElement(By.TagName("a")).GetAttribute("href").Trim();
                }
                catch
                { }
                    
                meetingsList.Add(new Meeting()
                {
                    MeetingID = ("Johnson-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                    MeetingType = meetingType,
                    MeetingDate = meetingDate,
                    MeetingTime = meetingTime,
                    Jurisdiction = "Johnson County",
                    State = "KS",
                    County = "Johnson",
                    AgendaURL = agendaURL
                });
            }

            IWebElement recentMeetings = mainPageDriver.FindElement(By.CssSelector("#meeting-list-recent"));

            var recentMeetingsOutputRows = recentMeetings.FindElements(By.CssSelector(".meeting-row"));

            foreach (var outputRow in recentMeetingsOutputRows)
            {
                string meetingType = outputRow.FindElements(By.TagName("td"))[0].Text.Trim();
                string meetingDateRaw = outputRow.FindElements(By.TagName("td"))[2].Text.Trim();
                string meetingDate = DateTime.ParseExact(meetingDateRaw.Split(" ")[0], "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd"); ;
                string meetingTime = meetingDateRaw.Split(" ")[1];
                string agendaURL = string.Empty;
                try
                {
                    agendaURL = outputRow.FindElements(By.TagName("td"))[5].FindElement(By.TagName("a")).GetAttribute("href").Trim();
                }
                catch
                { }

                meetingsList.Add(new Meeting()
                {
                    MeetingID = ("Johnson-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                    MeetingType = meetingType,
                    MeetingDate = meetingDate,
                    MeetingTime = meetingTime,
                    Jurisdiction = "Johnson County",
                    State = "KS",
                    County = "Johnson",
                    AgendaURL = agendaURL
                });
            }

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);

            mainPageDriver.Quit();
        }
    }
}