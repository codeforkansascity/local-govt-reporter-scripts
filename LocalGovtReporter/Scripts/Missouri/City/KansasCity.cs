using LocalGovtReporter.Interfaces;
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
    public class KansasCity : IScript
    {
        public async Task RunScriptAsync()
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
                    string meetingSource = outputRow.FindElements(By.TagName("td"))[1].FindElement(By.TagName("a")).GetAttribute("href").Trim();
                    string meetingTime = outputRow.FindElements(By.TagName("td"))[2].Text.Trim();
                    string agendaURL = outputRow.FindElements(By.TagName("td"))[4].FindElement(By.TagName("a")).GetAttribute("href").Trim();
                    string minutesURL = string.Empty;

                    string meetingLocation = string.Empty;
                    string meetingAddress = string.Empty;
                    string latitude = string.Empty;
                    string longitude = string.Empty;

                    if (HelperMethods.ContainsTag(outputRow.FindElements(By.TagName("td"))[5], "a"))
                        minutesURL = outputRow.FindElements(By.TagName("td"))[5].FindElement(By.TagName("a")).GetAttribute("href").Trim();

                    if (!string.IsNullOrEmpty(agendaURL))
                    {
                        try
                        {
                            var agendaText = HelperMethods.ReadPdfFile(agendaURL);

                            if (agendaText.Contains("Zoom") || agendaText.Contains("ZOOM") || agendaText.Contains("zoom"))
                            {
                                meetingLocation = "Remote Meeting";
                            }
                            else if (agendaText.Contains("Teams") || agendaText.Contains("TEAMS") || agendaText.Contains("teams"))
                            {
                                meetingLocation = "Remote Meeting";
                            }
                            else if (agendaText.Contains("Google") || agendaText.Contains("GOOGLE"))
                            {
                                meetingLocation = "Remote Meeting";
                            }
                            else if (agendaText.Contains("Call-in") || agendaText.Contains("Conference"))
                            {
                                meetingLocation = "Conference Call";
                            }
                            else if (agendaText.Contains("Board of Police Commissioners Meeting"))
                            {
                                meetingLocation = "KCPD Headquarters, Community Room";
                                meetingAddress = "1125 Locust St, Kansas City, MO 64106";
                                latitude = "39.100558";
                                longitude = "-94.577261";
                            }
                        }
                        catch
                        {
                            try
                            {
                                var agendaText = HelperMethods.ReadWordFile(agendaURL);

                                if (agendaText.Contains("Zoom") || agendaText.Contains("ZOOM") || agendaText.Contains("zoom"))
                                {
                                    meetingLocation = "Remote Meeting";
                                }
                                else if (agendaText.Contains("Teams") || agendaText.Contains("TEAMS") || agendaText.Contains("teams"))
                                {
                                    meetingLocation = "Remote Meeting";
                                }
                            }
                            catch
                            {

                            }
                        }
                    }

                    meetingsList.Add(new Meeting()
                    {
                        SourceURL = meetingSource,
                        MeetingID = ("KCMO-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                        MeetingType = meetingType,
                        MeetingDate = meetingDate,
                        MeetingTime = meetingTime,
                        MeetingLocation = meetingLocation,
                        MeetingAddress = meetingAddress,
                        Latitude = latitude,
                        Longitude = longitude,
                        Jurisdiction = "KCMO",
                        State = "MO",
                        County = "Jackson",
                        AgendaURL = agendaURL,
                        MinutesURL = minutesURL,
                        Tags = HelperMethods.CreateTags(meetingType)
                    });
                }
            }

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);

            mainPageDriver.Quit();
        }
    }
}