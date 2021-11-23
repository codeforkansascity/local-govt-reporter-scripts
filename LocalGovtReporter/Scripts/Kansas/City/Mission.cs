using LocalGovtReporter.Interfaces;
using LocalGovtReporter.Methods;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Xml;

namespace LocalGovtReporter.Scripts.Kansas.City
{
    public class Mission : IScript
    {
        public string AgencyName { get { return "City of Mission"; } }
        public string SiteURL { get { return "https://www.missionks.org/agenda.aspx"; } }
        public string SiteURL2 { get { return "https://www.missionks.org/city-government/agendas-minutes/"; } }

        public async Task<int> RunScriptAsync()
        {
            IWebDriver mainPageDriver = new ChromeDriver();
            IWebDriver subPageDriver = new ChromeDriver();

            List<Meeting> meetingsList = new List<Meeting>();
            ParseRSSdotnet();

            mainPageDriver.Navigate().GoToUrl(SiteURL);
            mainPageDriver.Navigate().GoToUrl(SiteURL2);


            var linksContainer = mainPageDriver.FindElement(By.CssSelector(".et_pb_text_inner"));
            //var linksContainer = mainPageDriver.FindElement(By.CssSelector("#aspnetForm > div.mainContainer > div.mainContainer > div.secondContainer > div.secondRight > div.pageLeftNoHigh"));
            ReadOnlyCollection<IWebElement> links = linksContainer.FindElements(By.TagName("a"));

            foreach (var link in links)
            {
                subPageDriver.Navigate().GoToUrl(link.GetAttribute("href"));
                var futureButton = subPageDriver.FindElement(By.CssSelector(".tribe-events-c-top-bar__today-button"));
                futureButton.Click();
                Thread.Sleep(2000); //Hopefully allow time for page to re-render
                var linksContainer2 = subPageDriver.FindElement(By.CssSelector(".tribe-events-calendar-list"));
                ReadOnlyCollection<IWebElement> links2 = linksContainer2.FindElements(By.CssSelector(".tribe-events-calendar-list__event-row"));
                foreach (var link2 in links2)
                {
                    var a0 = link2.FindElement(By.CssSelector(".tribe-events-calendar-list__event-details"));
                    var aa0 = link2.FindElements(By.CssSelector(".tribe-events-calendar-list__event-datetime-wrapper"));
                    aa0 = link2.FindElements(By.CssSelector(".tribe-events-calendar-list__event-details"));
                    foreach (var lg in aa0)
                    {
                        var el = lg.FindElement(By.TagName("span"));
                        var el1 = el.GetAttribute("innerHTML");
                        var a1 = lg.FindElement(By.TagName("a"));
                        var a11 = el.GetAttribute("href");
                        var a12 = el.GetAttribute("title");
                    }
                    link.FindElements(By.TagName("button"))[0].FindElement(By.ClassName("tribe-events-c-top-bar__datepicker-button")).Click();

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
                        string meetingLocation = string.Empty;
                        string meetingAddress = string.Empty;
                        string latitude = string.Empty;
                        string longitude = string.Empty;

                        string agendaName = meeting.FindElement(By.CssSelector(".agendaTitle")).Text.Trim();
                        ReadOnlyCollection<IWebElement> meetingLinks = meeting.FindElements(By.CssSelector(".agendaLink a"));

                        Match match = Regex.Match(agendaName, @"\d{0,2}\-\d{0,2}\-\d{2}");
                        string date = match.Value.Trim();

                        string meetingType;
                        if (!string.IsNullOrEmpty(date))
                        {
                            meetingType = agendaName.Replace(date, "").Trim();
                            meetingDate = DateTime.Parse(date).ToString("yyyy-MM-dd");
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

                        try
                        {
                            var agendaText = HelperMethods.ReadPdfFile(agendaURL);

                            if (agendaText.Contains("Zoom") || agendaText.Contains("ZOOM") || agendaText.Contains("zoom"))
                            {
                                meetingLocation = "Remote Meeting";
                            }
                            else
                            {
                                meetingLocation = "City Hall";
                                meetingAddress = "6090 Woodson Rd, Mission, KS 66202";
                                latitude = "39.019020";
                                longitude = "-94.654040";
                            }
                        }
                        catch
                        {

                        }

                        HelperMethods.AddMeeting(meetingsList, subPageDriver.Url, "Mission", meetingType, meetingDate, null, meetingLocation, meetingAddress, latitude, longitude, "KS", "Johnson", agendaURL, minutesURL, packetURL, videoURL);
                    }
                }
            }

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList, AgencyName);

            mainPageDriver.Quit();
            subPageDriver.Quit();
            return meetingsList.Count;
        }
        public void ParseRSSdotnet()
        {
            SyndicationFeed feed = null;

            try
            {
                using (var reader = XmlReader.Create("https://www.missionks.org/events/feed/"))
                {
                    feed = SyndicationFeed.Load(reader);
                }
            }
            catch { } // TODO: Deal with unavailable resource.

            if (feed != null)
            {
                foreach (var element in feed.Items)
                {
                    Console.WriteLine($"Title: {element.Title.Text}");
                    Console.WriteLine($"Summary: {element.Summary.Text}");
                    Console.WriteLine($"Summary: {element.Links[0].Uri}");
                    Console.WriteLine($"Summary: {element.PublishDate.DateTime}");
                }
            }
        }

    }


}