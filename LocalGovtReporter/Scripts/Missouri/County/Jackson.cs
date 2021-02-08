using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Missouri.County
{
    class Jackson
    {
        private static string FindVideoCode(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Regex pattern = new Regex(@"\b[0-9]{4}\b");
                Match match = pattern.Match(text);
                if (match.Success)
                    return match.Value;
                else
                    return string.Empty;
            }
            return string.Empty;
        }

        private static async Task ReadTableAsync(IWebDriver driver)
        {
            ReadOnlyCollection<IWebElement> tableRows = driver.FindElement(By.CssSelector(".rgMasterTable")).FindElements(By.CssSelector(".rgRow"));
            ReadOnlyCollection<IWebElement> tableAltRows = driver.FindElement(By.CssSelector(".rgMasterTable")).FindElements(By.CssSelector(".rgAltRow"));

            List<Meeting> meetingsList = new List<Meeting>();

            foreach (var tableRow in tableRows)
            {
                string meetingType = tableRow.FindElements(By.TagName("td"))[0].Text.Trim();
                string meetingDateRaw = tableRow.FindElements(By.TagName("td"))[1].Text.Trim();
                string meetingDate = DateTime.ParseExact(meetingDateRaw, "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                string meetingTime = tableRow.FindElements(By.TagName("td"))[3].Text.Trim();
                string meetingLocation = tableRow.FindElements(By.TagName("td"))[4].Text.Trim();
                string meetingAgenda = tableRow.FindElements(By.TagName("td"))[6].FindElement(By.TagName("a")).GetAttribute("href");
                string meetingMinutes = tableRow.FindElements(By.TagName("td"))[7].FindElement(By.TagName("a")).GetAttribute("href");
                string meetingVideoCode = FindVideoCode(tableRow.FindElements(By.TagName("td"))[8].FindElement(By.TagName("a")).GetAttribute("onclick"));
                string meetingVideo = "http://jacksonco.granicus.com/player/clip/" + meetingVideoCode;

                meetingsList.Add(new Meeting()
                {
                    MeetingID = ("Jackson-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                    MeetingType = meetingType,
                    MeetingDate = meetingDate,
                    MeetingTime = meetingTime,
                    MeetingLocation = meetingLocation,
                    Jurisdiction = "Jackson County",
                    State = "MO",
                    County = "Jackson",
                    AgendaURL = meetingAgenda,
                    MinutesURL = meetingMinutes == null ? "" : meetingMinutes,
                    VideoURL = meetingVideo == null ? "" : meetingVideo
                });
            }

            foreach (var tableAltRow in tableAltRows)
            {
                string meetingType = tableAltRow.FindElements(By.TagName("td"))[0].Text.Trim();
                string meetingDateRaw = tableAltRow.FindElements(By.TagName("td"))[1].Text.Trim();
                string meetingDate = DateTime.ParseExact(meetingDateRaw, "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                string meetingTime = tableAltRow.FindElements(By.TagName("td"))[3].Text.Trim();
                string meetingLocation = tableAltRow.FindElements(By.TagName("td"))[4].Text.Trim();
                string meetingAgenda = tableAltRow.FindElements(By.TagName("td"))[6].FindElement(By.TagName("a")).GetAttribute("href");
                string meetingMinutes = tableAltRow.FindElements(By.TagName("td"))[7].FindElement(By.TagName("a")).GetAttribute("href");
                string meetingVideoCode = FindVideoCode(tableAltRow.FindElements(By.TagName("td"))[8].FindElement(By.TagName("a")).GetAttribute("onclick"));
                string meetingVideo = "http://jacksonco.granicus.com/player/clip/" + meetingVideoCode;

                meetingsList.Add(new Meeting()
                {
                    MeetingID = ("Jackson-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                    MeetingType = meetingType,
                    MeetingDate = meetingDate,
                    MeetingTime = meetingTime,
                    MeetingLocation = meetingLocation,
                    Jurisdiction = "Jackson County",
                    State = "MO",
                    County = "Jackson",
                    AgendaURL = meetingAgenda,
                    MinutesURL = meetingMinutes == null ? "" : meetingMinutes,
                    VideoURL = meetingVideo == null ? "" : meetingVideo
                });
            }

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);
        }

        public static async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            mainPageDriver.Navigate().GoToUrl("https://jacksonco.legistar.com/Calendar.aspx");

            //ReadOnlyCollection<IWebElement> paginationButtons = mainPageDriver.FindElement(By.CssSelector(".rgNumPart")).FindElements(By.TagName("a"));

            //foreach (var paginationButton in paginationButtons)
            //{
            //    paginationButton.Click();
            //    Thread.Sleep(5000);
                await ReadTableAsync(mainPageDriver);
            //}

            mainPageDriver.Quit();
        }
    }
}