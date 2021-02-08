using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Missouri.City
{
    public static class KansasCity
    {
        public static bool ContainsTag(this IWebElement element, string tagName)
        {
            string elementText = element.GetAttribute("innerHTML");
            return CheckStringForTag(elementText, tagName);
        }

        public static bool CheckStringForTag(string text, string tagName)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Contains("<" + tagName + ">") || text.Contains("</" + tagName + ">") || text.Contains("<" + tagName + " ");
            }
            return false;
        }

        public static async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl("http://cityclerk.kcmo.org/liveweb/Meetings/HistoricalMeetings.aspx");

            ReadOnlyCollection<IWebElement> monthTables = mainPageDriver.FindElements(By.CssSelector(".monthTable"));

            foreach (var monthTable in monthTables)
            {
                var outputRows = monthTable.FindElements(By.CssSelector(".outputRow"));

                foreach (var outputRow in outputRows)
                {
                    string meetingDateRaw = outputRow.FindElements(By.TagName("td"))[0].Text.Trim();
                    string meetingDate = DateTime.ParseExact(meetingDateRaw, "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                    string meetingType = outputRow.FindElements(By.TagName("td"))[1].Text.Trim();
                    string meetingTime = outputRow.FindElements(By.TagName("td"))[2].Text.Trim();
                    string agendaURL = outputRow.FindElements(By.TagName("td"))[4].FindElement(By.TagName("a")).GetAttribute("href").Trim();

                    string minutesURL = string.Empty;

                    if (ContainsTag(outputRow.FindElements(By.TagName("td"))[5], "a"))
                        minutesURL = outputRow.FindElements(By.TagName("td"))[5].FindElement(By.TagName("a")).GetAttribute("href").Trim();

                    meetingsList.Add(new Meeting()
                    {
                        MeetingID = ("KCMO-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        MeetingTime = meetingTime,
                        Jurisdiction = "KCMO",
                        State = "MO",
                        County = "Jackson",
                        AgendaURL = agendaURL,
                        MinutesURL = minutesURL
                    });
                }
            }

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);

            mainPageDriver.Quit();
        }
    }
}