using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Kansas.County
{
    class Wyandotte
    {
        public static async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl("https://wycokck.civicclerk.com/web/home.aspx");
            Thread.Sleep(5000);

            IWebElement upcomingMeetings = mainPageDriver.FindElement(By.CssSelector("#aspxroundpanelCurrent"));

            var upcomingMeetingsOutputRows = upcomingMeetings.FindElements(By.CssSelector(".dxgvDataRow_CustomThemeModerno"));

            foreach (var outputRow in upcomingMeetingsOutputRows)
            {
                try
                {                   
                    string meetingDateRaw = outputRow.FindElements(By.TagName("td"))[1].Text.Trim();
                    string meetingDate = DateTime.ParseExact(meetingDateRaw.Split(" ")[0], "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                    string meetingType = outputRow.FindElements(By.TagName("td"))[0].Text.Trim().Replace(DateTime.ParseExact(meetingDateRaw.Split(" ")[0], "M/d/yyyy", CultureInfo.CurrentCulture).ToString("M/d/yy"), "").Trim();
                    string meetingTime = meetingDateRaw.Split(" ")[1].Trim('*').Trim() + " " + meetingDateRaw.Split(" ")[2].Trim('*').Trim();

                    meetingsList.Add(new Meeting()
                    {
                        MeetingID = ("Wyandotte-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        MeetingTime = meetingTime,
                        Jurisdiction = "Wyandotte County",
                        State = "KS",
                        County = "Wyandotte"
                    });
                }
                catch
                {
                }
            }

            IWebElement recentMeetings = mainPageDriver.FindElement(By.CssSelector("#aspxroundpanelRecent2"));

            var recentMeetingsOutputRows = recentMeetings.FindElements(By.CssSelector(".dxgvDataRow_CustomThemeModerno"));

            foreach (var outputRow in recentMeetingsOutputRows)
            {
                try
                {
                    string meetingDateRaw = outputRow.FindElements(By.TagName("td"))[1].Text.Trim();
                    string meetingDate = DateTime.ParseExact(meetingDateRaw.Split(" ")[0], "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                    string meetingType = outputRow.FindElements(By.TagName("td"))[0].Text.Trim().Replace(DateTime.ParseExact(meetingDateRaw.Split(" ")[0], "M/d/yyyy", CultureInfo.CurrentCulture).ToString("M/d/yy"), "").Trim();
                    string meetingTime = meetingDateRaw.Split(" ")[1].Trim('*').Trim() + " " + meetingDateRaw.Split(" ")[2].Trim('*').Trim();

                    meetingsList.Add(new Meeting()
                    {
                        MeetingID = ("Wyandotte-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        MeetingTime = meetingTime,
                        Jurisdiction = "Wyandotte County",
                        State = "KS",
                        County = "Wyandotte"
                    });
                }
                catch
                {
                }
            }

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);

            mainPageDriver.Quit();
        }
    }
}