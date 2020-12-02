using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
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
                    string meetingDate = outputRow.FindElements(By.TagName("td"))[0].Text.Trim();
                    string meetingType = outputRow.FindElements(By.TagName("td"))[1].Text.Trim();
                    string meetingTime = outputRow.FindElements(By.TagName("td"))[2].Text.Trim();
                    string agendaURL = outputRow.FindElements(By.TagName("td"))[4].FindElement(By.TagName("a")).GetAttribute("href").Trim();

                    string minutesURL = string.Empty;

                    if (ContainsTag(outputRow.FindElements(By.TagName("td"))[5], "a"))
                        minutesURL = outputRow.FindElements(By.TagName("td"))[5].FindElement(By.TagName("a")).GetAttribute("href").Trim();

                    meetingsList.Add(new Meeting()
                    {
                        MeetingID = meetingType + " " + meetingDate + " KCMO",
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

            var credentials = new BasicAWSCredentials("accessKey", "secretKey");

            foreach (var meeting in meetingsList)
            {
                using (var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2))
                {
                    await client.PutItemAsync(new PutItemRequest
                    {
                        TableName = "Meeting",
                        Item = new Dictionary<string, AttributeValue>()
                        {
                            { "MeetingID", new AttributeValue { S = meeting.MeetingID }},
                            { "MeetingType", new AttributeValue { S = meeting.MeetingType }},
                            { "Jurisdiction", new AttributeValue { S = meeting.Jurisdiction }},
                            { "State", new AttributeValue { S = meeting.State }},
                            { "County", new AttributeValue { S = meeting.County }},
                            { "MeetingDate", new AttributeValue { S = meeting.MeetingDate }},
                            { "MeetingTime", new AttributeValue { S = meeting.MeetingTime }},
                            { "AgendaURL", new AttributeValue { S = meeting.AgendaURL }},
                            { "MinutesURL", new AttributeValue { S = meeting.MinutesURL }}
                        }
                    });
                }
            }

            mainPageDriver.Quit();
        }
    }
}
