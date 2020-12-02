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

namespace LocalGovtReporter.Scripts.Kansas.City
{
    class Mission
    {
        public static async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();
            IWebDriver subPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl("https://www.missionks.org/agenda.aspx");

            var linksContainer = mainPageDriver.FindElement(By.CssSelector("#aspnetForm > div.mainContainer > div.mainContainer > div.secondContainer > div.secondRight > div.pageLeftNoHigh"));
            ReadOnlyCollection<IWebElement> links = linksContainer.FindElements(By.CssSelector(".floatLeft a"));

            foreach (var link in links)
            {
                subPageDriver.Navigate().GoToUrl(link.GetAttribute("href"));
                var meetingTypeRawText = subPageDriver.FindElement(By.CssSelector("div.noteBox:nth-child(1) > span:nth-child(1)")).Text.Trim();
                var meetingTypeCleanedText = meetingTypeRawText.Substring(0, meetingTypeRawText.IndexOf('(')).Trim();
                var currentMeetingsContainer = subPageDriver.FindElement(By.CssSelector(".pageLeftNoHigh > div:nth-child(8)"));
                ReadOnlyCollection<IWebElement> meetings = currentMeetingsContainer.FindElements(By.CssSelector(".itemLineConSM"));

                foreach (var meeting in meetings)
                {
                    string meetingDate = string.Empty;
                    string agendaURL = string.Empty;
                    string packetURL = string.Empty;
                    string videoURL = string.Empty;
                    string minutesURL = string.Empty;

                    string agendaName = meeting.FindElement(By.CssSelector(".agendaTitle")).Text.Trim();
                    ReadOnlyCollection<IWebElement> meetingLinks = meeting.FindElements(By.CssSelector(".agendaLink a"));

                    Match match = Regex.Match(agendaName, @"\d{0,2}\-\d{0,2}\-\d{2}");
                    string date = match.Value.Trim();

                    string meetingType;
                    if (!string.IsNullOrEmpty(date))
                    {
                        meetingType = agendaName.Replace(date, "").Trim();
                        meetingDate = DateTime.Parse(date).ToShortDateString();
                    }
                    else
                    {
                        meetingType = agendaName.Trim();
                    }

                    foreach (var meetingLink in meetingLinks)
                    {
                        if (meetingLink.Text == "Agenda")
                            agendaURL = meetingLink.GetAttribute("href").Trim();
                        else if (meetingLink.Text == "Packet")
                            packetURL = meetingLink.GetAttribute("href").Trim();
                        else if (meetingLink.Text == "Video")
                            videoURL = meetingLink.GetAttribute("href").Trim();
                        else if (meetingLink.Text == "Minutes")
                            minutesURL = meetingLink.GetAttribute("href").Trim();
                    }

                    meetingsList.Add(new Meeting() { 
                        MeetingID = meetingType + " " + meetingDate + " Mission", 
                        MeetingType = meetingType, 
                        MeetingDate = meetingDate,
                        Jurisdiction = "Mission",
                        State = "KS",
                        County = "Johnson",
                        AgendaURL = agendaURL, 
                        MinutesURL = minutesURL, 
                        PacketURL = packetURL, 
                        VideoURL = videoURL 
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
                            { "AgendaURL", new AttributeValue { S = meeting.AgendaURL }},
                            { "PacketURL", new AttributeValue { S = meeting.PacketURL }},
                            { "VideoURL", new AttributeValue { S = meeting.VideoURL }},
                            { "MinutesURL", new AttributeValue { S = meeting.MinutesURL }}
                        }
                    });
                }
            }

            mainPageDriver.Quit();
            subPageDriver.Quit();
        }
    }
}