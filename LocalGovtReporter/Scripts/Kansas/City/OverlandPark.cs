using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace LocalGovtReporter.Scripts.Kansas.City
{
    public class OverlandPark : IScript
    {
        public async Task RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();
            IWebDriver subPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();

            mainPageDriver.Navigate().GoToUrl("https://opkansas.civicweb.net/Portal/MeetingTypeList.aspx");
            Thread.Sleep(5000);

            var upcomingMeetings = mainPageDriver.FindElement(By.CssSelector(".upcoming-meeting-list"));
            ReadOnlyCollection<IWebElement> linkedUpcomingMeetings = upcomingMeetings.FindElements(By.CssSelector(".list-link"));

            foreach (var link in linkedUpcomingMeetings)
            {
                string meetingType = string.Empty;
                string meetingDate = string.Empty;

                if (link.Text.Split(" - ").Length > 0)
                {
                    meetingType = link.Text.Split(" - ")[0];
                    meetingDate = DateTime.Parse(link.Text.Split(" - ")[1]).ToString("yyyy-MM-dd");
                }

                subPageDriver.Navigate().GoToUrl(link.GetAttribute("href"));
                Thread.Sleep(3000);

                string meetingTime = subPageDriver.FindElement(By.CssSelector("#meeting-time")).Text;
                string meetingLocation = subPageDriver.FindElement(By.CssSelector("#meeting-location")).Text;

                meetingsList.Add(new Meeting()
                {
                    SourceURL = subPageDriver.Url,
                    MeetingID = ("Overland-Park-" + meetingDate + "-" + meetingType).Replace(" ", "-").Replace(",", ""),
                    MeetingType = meetingType,
                    MeetingDate = meetingDate,
                    MeetingTime = meetingTime,
                    MeetingLocation = meetingLocation,
                    Jurisdiction = "Overland Park",
                    State = "KS",
                    County = "Johnson",
                    AgendaURL = subPageDriver.FindElement(By.CssSelector("#document-cover-pdf")).GetAttribute("href"),
                    PacketURL = subPageDriver.FindElement(By.CssSelector("#ctl00_MainContent_DocumentPrintVersion")).GetAttribute("href"),
                    Tags = HelperMethods.CreateTags(meetingType)
                });
            }

            ReadOnlyCollection<IWebElement> unlinkedUpcomingMeetings = upcomingMeetings.FindElements(By.XPath("//li[@title='Final version not published']"));

            foreach (var link in unlinkedUpcomingMeetings)
            {
                if (link.FindElements(By.TagName("span"))[1].Text.Length > 0)
                {
                    string meetingType = string.Empty;
                    string meetingDate = string.Empty;

                    if (link.FindElements(By.TagName("span"))[1].Text.Split(" - ").Length > 0)
                    {
                        meetingType = link.FindElements(By.TagName("span"))[1].Text.Split(" - ")[0];
                        meetingDate = DateTime.Parse(link.FindElements(By.TagName("span"))[1].Text.Split(" - ")[1]).ToString("yyyy-MM-dd");
                    }

                    meetingsList.Add(new Meeting()
                    {
                        SourceURL = subPageDriver.Url,
                        MeetingID = ("Overland-Park-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        Jurisdiction = "Overland Park",
                        State = "KS",
                        County = "Johnson",
                        Tags = HelperMethods.CreateTags(meetingType)
                    });
                }
            }

            var recentMeetings = mainPageDriver.FindElement(By.CssSelector(".recent-meeting-list"));
            ReadOnlyCollection<IWebElement> linkedRecentMeetings = recentMeetings.FindElements(By.CssSelector(".list-link"));

            foreach (var link in linkedRecentMeetings)
            {
                try
                {
                    string meetingType = string.Empty;
                    string meetingDate = string.Empty;

                    if (link.Text.Split(" - ").Length > 0)
                    {
                        meetingType = link.Text.Split(" - ")[0];
                        meetingDate = DateTime.Parse(link.Text.Split(" - ")[1]).ToString("yyyy-MM-dd");
                    }

                    subPageDriver.Navigate().GoToUrl(link.GetAttribute("href"));
                    Thread.Sleep(3000);

                    string meetingTime = subPageDriver.FindElement(By.CssSelector("#meeting-time")).Text;
                    string meetingLocation = subPageDriver.FindElement(By.CssSelector("#meeting-location")).Text;
                    string minutesURL = string.Empty;

                    if (subPageDriver.FindElements(By.CssSelector("#ctl00_MainContent_MinutesDocument")).Count > 0)
                    {
                        subPageDriver.FindElement(By.CssSelector("#ctl00_MainContent_MinutesDocument")).Click();
                        minutesURL = subPageDriver.FindElement(By.CssSelector("#ctl00_MainContent_DocumentPrintVersion")).GetAttribute("href");
                        subPageDriver.FindElement(By.CssSelector("#ctl00_MainContent_AgendaDocument")).Click();
                    }

                    meetingsList.Add(new Meeting()
                    {
                        SourceURL = subPageDriver.Url,
                        MeetingID = ("Overland-Park-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        MeetingTime = meetingTime,
                        MeetingLocation = meetingLocation,
                        Jurisdiction = "Overland Park",
                        State = "KS",
                        County = "Johnson",
                        MinutesURL = minutesURL,
                        VideoURL = subPageDriver.FindElement(By.CssSelector("#ExternalLink")).GetAttribute("href"),
                        AgendaURL = subPageDriver.FindElement(By.CssSelector("#document-cover-pdf")).GetAttribute("href"),
                        PacketURL = subPageDriver.FindElement(By.CssSelector("#ctl00_MainContent_DocumentPrintVersion")).GetAttribute("href"),
                        Tags = HelperMethods.CreateTags(meetingType)
                    });
                }
                catch
                {

                }
            }

            ReadOnlyCollection<IWebElement> unlinkedRecentMeetings = recentMeetings.FindElements(By.XPath("//li[@title='Final version not published']"));

            foreach (var link in unlinkedRecentMeetings)
            {
                if (link.FindElements(By.TagName("span"))[1].Text.Length > 0)
                {
                    string meetingType = string.Empty;
                    string meetingDate = string.Empty;

                    if (link.FindElements(By.TagName("span"))[1].Text.Split(" - ").Length > 0)
                    {
                        meetingType = link.FindElements(By.TagName("span"))[1].Text.Split(" - ")[0];
                        meetingDate = DateTime.Parse(link.FindElements(By.TagName("span"))[1].Text.Split(" - ")[1]).ToString("yyyy-MM-dd");
                    }

                    meetingsList.Add(new Meeting()
                    {
                        SourceURL = subPageDriver.Url,
                        MeetingID = ("Overland-Park-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        Jurisdiction = "Overland Park",
                        State = "KS",
                        County = "Johnson",
                        Tags = HelperMethods.CreateTags(meetingType)
                    });
                }
            }

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);

            mainPageDriver.Quit();
            subPageDriver.Quit();
        }    
    }
}