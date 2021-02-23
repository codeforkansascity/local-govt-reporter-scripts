using LocalGovtReporter.Interfaces;
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
    public class Wyandotte : IScript
    {
        public async Task RunScriptAsync()
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
                    string meetingSourceID = outputRow.FindElements(By.TagName("td"))[0].FindElement(By.TagName("a")).GetAttribute("href").Replace("javascript:LaunchPlayer(", "").Trim(';').Trim(')').Split(",")[0];
                    string meetingSource = "https://wycokck.civicclerk.com/Web/Player.aspx?id=" + meetingSourceID + "&key=-1&mod=-1&mk=-1&nov=0";
                    string meetingTime = meetingDateRaw.Split(" ")[1].Trim('*').Trim() + " " + meetingDateRaw.Split(" ")[2].Trim('*').Trim();

                    outputRow.FindElements(By.TagName("td"))[3].FindElement(By.ClassName("dxic")).Click();
                    Thread.Sleep(1000);
                    outputRow.FindElements(By.TagName("td"))[3].FindElement(By.ClassName("dxlbd")).Click();

                    var browserTabs = new List<string>(mainPageDriver.WindowHandles);
                    var newTab = browserTabs[1];
                    string agendaURL = mainPageDriver.SwitchTo().Window(newTab).Url;

                    if (browserTabs.Count > 1)
                    {
                        mainPageDriver.SwitchTo().Window(browserTabs[1]);
                        mainPageDriver.Close();
                        mainPageDriver.SwitchTo().Window(browserTabs[0]);
                    }

                    meetingsList.Add(new Meeting()
                    {
                        SourceURL = meetingSource,
                        MeetingID = ("Wyandotte-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        MeetingTime = meetingTime,
                        Jurisdiction = "Wyandotte County",
                        AgendaURL = agendaURL,
                        MeetingLocation = "Remote Meeting",
                        //MeetingAddress = "701 N 7th St, Suite 323 Kansas City, KS 66101",
                        //Latitude = "39.113020",
                        //Longitude = "-94.626740",
                        State = "KS",
                        County = "Wyandotte",
                        Tags = HelperMethods.CreateTags(meetingType)
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
                    string meetingSourceID = outputRow.FindElements(By.TagName("td"))[0].FindElement(By.TagName("a")).GetAttribute("href").Replace("javascript:LaunchPlayer(", "").Trim(';').Trim(')').Split(",")[0];
                    string meetingSource = "https://wycokck.civicclerk.com/Web/Player.aspx?id=" + meetingSourceID + "&key=-1&mod=-1&mk=-1&nov=0";
                    string meetingTime = meetingDateRaw.Split(" ")[1].Trim('*').Trim() + " " + meetingDateRaw.Split(" ")[2].Trim('*').Trim();
                    string agendaURL = string.Empty;

                    outputRow.FindElements(By.TagName("td"))[3].FindElement(By.ClassName("dxic")).Click();
                    Thread.Sleep(1000);
                    outputRow.FindElements(By.TagName("td"))[3].FindElement(By.ClassName("dxlbd")).Click();

                    var browserTabs = new List<string>(mainPageDriver.WindowHandles);
                    var newTab = browserTabs[1];
                    agendaURL = mainPageDriver.SwitchTo().Window(newTab).Url;

                    if (browserTabs.Count > 1)
                    {
                        mainPageDriver.SwitchTo().Window(browserTabs[1]);
                        mainPageDriver.Close();
                        mainPageDriver.SwitchTo().Window(browserTabs[0]);
                    }

                    meetingsList.Add(new Meeting()
                    {
                        SourceURL = meetingSource,
                        MeetingID = ("Wyandotte-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        MeetingTime = meetingTime,
                        Jurisdiction = "Wyandotte County",
                        AgendaURL = agendaURL,
                        MeetingLocation = "Remote Meeting",
                        //MeetingAddress = "701 N 7th St, Suite 323 Kansas City, KS 66101",
                        //Latitude = "39.113020",
                        //Longitude = "-94.626740",
                        State = "KS",
                        County = "Wyandotte",
                        Tags = HelperMethods.CreateTags(meetingType)
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