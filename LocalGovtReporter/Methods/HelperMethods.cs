using DocumentFormat.OpenXml.Packaging;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using LocalGovtReporter.Models;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace LocalGovtReporter.Methods
{
    public static class HelperMethods
    {
        public static void MessageBuildingMeetingList(string agency)
		{
            System.Console.WriteLine($"Building meeting list for {agency}");
        }
        public static void MessageAddingMeetingList(string agency, int meetings)
        {
            System.Console.WriteLine($"Adding {meetings} for {agency}");
        }
        public static void AddToSummaryMessage(string agency, int meetings)
        {
            SummaryMessage += $"Added {meetings} for {agency}" + Environment.NewLine;
            SummaryMessage += "******************************";
        }
        public static string SummaryMessage { get; set; }
        public static void ErrorOnAgency(string agency, string msg)
        {
            SummaryMessage += $"Failed processing {agency}" + Environment.NewLine;
            SummaryMessage += msg + Environment.NewLine;
            SummaryMessage += "******************************";
            Console.WriteLine("******************************");
            Console.WriteLine($"Failed processing {agency}");
            Console.WriteLine(msg);
            Console.WriteLine("******************************");
        }
        public static void KCMOGetMeetings(IWebDriver driver, string rowSelector, List<Meeting> meetingsList)
        {
            ReadOnlyCollection<IWebElement> monthTables = driver.FindElements(By.CssSelector(".rgMasterTable"));
            int mmm = 0;
            foreach (var monthTable in monthTables)
            {
                mmm++;
                Console.WriteLine("Month table " + mmm + " of " + monthTables.Count);
                var outputRows = monthTable.FindElements(By.CssSelector(".rgRow"));
                if (mmm == 2)
                {
                    foreach (var outputRow in outputRows)
                    {
                        string meetingDateRaw = outputRow.FindElements(By.TagName("td"))[1].Text.Trim();
                        string meetingDate = DateTime.ParseExact(meetingDateRaw, "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                        string meetingType = outputRow.FindElements(By.TagName("td"))[0].Text.Trim();
                        string meetingSource = outputRow.FindElements(By.TagName("td"))[0].FindElement(By.TagName("a")).GetAttribute("href").Trim();
                        string meetingTime = outputRow.FindElements(By.TagName("td"))[3].Text.Trim();
                        string agendaURL = outputRow.FindElements(By.TagName("td"))[6].FindElement(By.TagName("a")).GetAttribute("href");
                        if (!string.IsNullOrWhiteSpace(agendaURL))
                            agendaURL = agendaURL.Trim();
                        string minutesURL = string.Empty; // outputRow.FindElements(By.TagName("td"))[8].FindElement(By.TagName("a")).GetAttribute("href").Trim(); ;

                        string meetingLocation = outputRow.FindElements(By.TagName("td"))[4].Text.Trim();
                        string meetingAddress = string.Empty;
                        string latitude = string.Empty;
                        string longitude = string.Empty;

                        if (ContainsTag(outputRow.FindElements(By.TagName("td"))[5], "a"))
                        {
                            minutesURL = outputRow.FindElements(By.TagName("td"))[5].FindElement(By.TagName("a")).GetAttribute("href");
                            if (!string.IsNullOrWhiteSpace(minutesURL))
                                minutesURL = minutesURL.Trim();
                        }
                        if (!string.IsNullOrEmpty(agendaURL))
                        {
                            try
                            {
                                var agendaText = ReadPdfFile(agendaURL);

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
                                    var agendaText = ReadWordFile(agendaURL);

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

                        AddMeeting(meetingsList, meetingSource, "KCMO", meetingType, meetingDate, meetingTime, meetingLocation, meetingAddress, latitude, longitude, "MO", "Jackson", agendaURL, minutesURL, null, null);
                    }
                }
            }
        }

        public static void JohnsonCountyGetMeetings(IWebDriver driver, string tableElementSelector, List<Meeting> meetingsList)
        {
            IWebElement meetingsTable = driver.FindElement(By.CssSelector(tableElementSelector));
            var meetingsRows = meetingsTable.FindElements(By.CssSelector(".meeting-row"));

            foreach (var outputRow in meetingsRows)
            {
                try
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
                    {

                    }

                    AddMeeting(meetingsList, driver.Url, "Johnson County", meetingType, meetingDate, meetingTime, "Remote Meeting", null, null, null, "KS", "Johnson", agendaURL, null, null, null);
                }
                catch
                {

                }
            }
        }

        public static void WyandotteCountyGetMeetings(IWebDriver driver, string tableElementSelector, List<Meeting> meetingsList)
        {
            IWebElement meetingsTable = driver.FindElement(By.CssSelector(tableElementSelector));
            var meetingsRows = meetingsTable.FindElements(By.CssSelector(".dxgvDataRow_CustomThemeModerno"));

            foreach (var outputRow in meetingsRows)
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

                    var browserTabs = new List<string>(driver.WindowHandles);
                    var newTab = browserTabs[1];
                    string agendaURL = driver.SwitchTo().Window(newTab).Url;

                    if (browserTabs.Count > 1)
                    {
                        driver.SwitchTo().Window(browserTabs[1]);
                        driver.Close();
                        driver.SwitchTo().Window(browserTabs[0]);
                    }

                    AddMeeting(meetingsList, driver.Url, "Wyandotte County", meetingType, meetingDate, meetingTime, "Remote Meeting", null, null, null, "KS", "Wyandotte", agendaURL, null, null, null);
                }
                catch
                {

                }
            }
        }

        public static void JacksonCountyGetMeetings(IWebDriver driver, string tableRowsSelector, List<Meeting> meetingsList)
        {
            ReadOnlyCollection<IWebElement> tableRows = driver.FindElement(By.CssSelector(".rgMasterTable")).FindElements(By.CssSelector(tableRowsSelector));

            foreach (var tableRow in tableRows)
            {
                try
                {
                    string meetingType = tableRow.FindElements(By.TagName("td"))[0].Text.Trim();
                    string meetingSource = tableRow.FindElements(By.TagName("td"))[0].FindElement(By.TagName("a")).GetAttribute("href");
                    string meetingDateRaw = tableRow.FindElements(By.TagName("td"))[1].Text.Trim();
                    string meetingDate = DateTime.ParseExact(meetingDateRaw, "M/d/yyyy", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                    string meetingTime = tableRow.FindElements(By.TagName("td"))[3].Text.Trim();
                    string meetingLocation = tableRow.FindElements(By.TagName("td"))[4].Text.Trim();
                    string meetingAgenda = tableRow.FindElements(By.TagName("td"))[6].FindElement(By.TagName("a")).GetAttribute("href");
                    string meetingMinutes = tableRow.FindElements(By.TagName("td"))[7].FindElement(By.TagName("a")).GetAttribute("href");
                    string meetingVideoCode = FindVideoCode(tableRow.FindElements(By.TagName("td"))[8].FindElement(By.TagName("a")).GetAttribute("onclick"));
                    string meetingVideo = string.Empty;

                    if (!string.IsNullOrEmpty(meetingVideoCode))
                        meetingVideo = "http://jacksonco.granicus.com/player/clip/" + meetingVideoCode;

                    AddMeeting(meetingsList, meetingSource, "Jackson County", meetingType, meetingDate, meetingTime, meetingLocation, "415 E 12th St, Kansas City, MO 64106", "39.099123", "-94.577986", "MO", "Jackson", meetingAgenda, meetingMinutes, null, meetingVideo);
                }
                catch
                {

                }
            }
        }

        public static void OverlandParkGetMeetings(IWebDriver mainDriver, IWebDriver subDriver, string containerSelector, List<Meeting> meetingsList)
        {
            var meetings = mainDriver.FindElement(By.CssSelector(containerSelector));
            ReadOnlyCollection<IWebElement> linkedMeetings = meetings.FindElements(By.CssSelector(".list-link"));

            foreach (var linkedMeeting in linkedMeetings)
            {
                try
                {
                    string meetingType = string.Empty;
                    string meetingDate = string.Empty;

                    if (linkedMeeting.Text.Split(" - ").Length > 0)
                    {
                        meetingType = linkedMeeting.Text.Split(" - ")[0];
                        meetingDate = DateTime.Parse(linkedMeeting.Text.Split(" - ")[1]).ToString("yyyy-MM-dd");
                    }

                    subDriver.Navigate().GoToUrl(linkedMeeting.GetAttribute("href"));
                    Thread.Sleep(3000);

                    string meetingTime = subDriver.FindElement(By.CssSelector("#meeting-time")).Text;
                    string meetingLocation = subDriver.FindElement(By.CssSelector("#meeting-location")).Text;
                    string meetingAddress = string.Empty;
                    string latitude = string.Empty;
                    string longitude = string.Empty;

                    if (meetingLocation.Contains("Convention Center"))
                    {
                        meetingAddress = "6000 College Blvd, Overland Park, KS 66211";
                        latitude = "38.916748";
                        longitude = "-94.656653";
                    }
                    else if (meetingLocation.Contains("City Hall"))
                    {
                        meetingAddress = "8500 Santa Fe Dr, Overland Park, KS 66212";
                        latitude = "38.974290";
                        longitude = "-94.685390";
                    }

                    AddMeeting(meetingsList, subDriver.Url, "Overland Park", meetingType, meetingDate, meetingTime, meetingLocation, meetingAddress, latitude, longitude, "KS", "Johnson", subDriver.FindElement(By.CssSelector("#document-cover-pdf")).GetAttribute("href"), subDriver.FindElement(By.CssSelector("#ctl00_MainContent_DocumentPrintVersion")).GetAttribute("href"), null, null);
                } 
                catch
                {

                }
            }

            ReadOnlyCollection<IWebElement> unlinkedMeetings = meetings.FindElements(By.XPath("//li[@title='Final version not published']"));

            foreach (var unlinkedMeeting in unlinkedMeetings)
            {
                try
                {
                    if (unlinkedMeeting.FindElements(By.TagName("span"))[1].Text.Length > 0)
                    {
                        string meetingType = string.Empty;
                        string meetingDate = string.Empty;

                        if (unlinkedMeeting.FindElements(By.TagName("span"))[1].Text.Split(" - ").Length > 0)
                        {
                            meetingType = unlinkedMeeting.FindElements(By.TagName("span"))[1].Text.Split(" - ")[0];
                            meetingDate = DateTime.Parse(unlinkedMeeting.FindElements(By.TagName("span"))[1].Text.Split(" - ")[1]).ToString("yyyy-MM-dd");
                        }

                        AddMeeting(meetingsList, subDriver.Url, "Overland Park", meetingType, meetingDate, null, null, null, null, null, "KS", "Johnson", null, null, null, null);
                    }
                }
                catch
                {

                }
            }
        }

        public static bool ContainsTag(this IWebElement element, string tagName)
        {
            string elementText = element.GetAttribute("innerHTML");
            return CheckStringForTag(elementText, tagName);
        }

        public static bool CheckStringForTag(string text, string tagName)
        {
            if (!string.IsNullOrWhiteSpace(text))
                return text.Contains("<" + tagName + ">") || text.Contains("</" + tagName + ">") || text.Contains("<" + tagName + " ");
            return false;
        }

        public static string ReadPdfFile(string url)
        {
            PdfReader reader = new PdfReader(new Uri(url));
            return PdfTextExtractor.GetTextFromPage(reader, 1);
        }

        public static string ReadWordFile(string url)
        {
            using (WebClient myWebClient = new WebClient())
            {
                byte[] bytes = myWebClient.DownloadData(url);
                MemoryStream memoryStream = new MemoryStream(bytes);
                MainDocumentPart mainPart;

                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(memoryStream, false))
                {
                    mainPart = wordDocument.MainDocumentPart;
                }

                return mainPart.Document.Body.InnerText;
            }
        }

        public static string FindVideoCode(string text)
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

        public static void AddMeeting(List<Meeting> meetingsList, string sourceURL, string jurisdiction, string meetingType, string meetingDate, string meetingTime, string meetingLocation, string meetingAddress, string latitude, string longitude, string state, string county, string agendaURL, string minutesURL, string packetURL, string videoURL)
        {
            meetingsList.Add(new Meeting()
            {
                SourceURL = sourceURL,
                Jurisdiction = jurisdiction,
                MeetingID = (jurisdiction + "-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                MeetingType = meetingType,
                MeetingDate = meetingDate,
                MeetingTime = meetingTime,
                MeetingLocation = meetingLocation,
                MeetingAddress = meetingAddress,
                Latitude = latitude,
                Longitude = longitude,
                State = state,
                County = county,
                AgendaURL = agendaURL,
                MinutesURL = minutesURL,
                PacketURL = packetURL,
                VideoURL = videoURL,
                Tags = CreateTags(meetingType)
            });
        }

        public static List<string> CreateTags(string meetingType)
        {
            List<string> tags = new List<string>();

            try
            {
                if (meetingType.Contains("Houseless"))
                    tags.Add("Other");
                if (meetingType == "Economic Development Corporation")
                    tags.Add("Other");
                if (meetingType == "Central City Economic Development Sales Tax Board")
                    tags.Add("Other");
                if (meetingType == "Municipal Officials and Officers Ethics Commission")
                    tags.Add("Other");
                if (meetingType.Contains("COVID"))
                    tags.Add("COVID");
                if (meetingType == "KC Parking and Transportation Commission")
                    tags.Add("Transportation");
                if (meetingType == "VFKCC")
                    tags.Add("Other");
                if (meetingType.Contains("Special"))
                    tags.Add("Special Session");
                if (meetingType.Contains("Council"))
                    tags.Add("Main Council Meeting");
                if (meetingType == "Sustainability Commission")
                    tags.Add("Other");
                if (meetingType.Contains("Parks"))
                    tags.Add("Parks and Rec");
                if (meetingType.Contains("Public Safety"))
                    tags.Add("Public Safety");
                if (meetingType.Contains("Public Works"))
                    tags.Add("Public Works");
                if (meetingType.Contains("Finance"))
                    tags.Add("Finance");
                if (meetingType.Contains("Planning"))
                    tags.Add("Planning");
                if (meetingType.Contains("Community Development"))
                    tags.Add("Community Development");
                if (meetingType.Contains("Downtown Business Improvement District Advisory Board"))
                    tags.Add("Other");
                if (meetingType.Contains("Community Development Block Grant Advisory Committee"))
                    tags.Add("Other");
                if (meetingType.Contains("Code Board of Appeals"))
                    tags.Add("Other");
                if (meetingType.Contains("Mental Health Task Force"))
                    tags.Add("Other");
                if (meetingType.Contains("Committee of the Whole"))
                    tags.Add("Committee of the Whole");
                if (meetingType.Contains("Overland Park Development Corporation"))
                    tags.Add("Other");
                if (meetingType == "F&A Committee")
                    tags.Add("Other");
                if (meetingType == "CDC")
                    tags.Add("Other");
                if (meetingType == "PBC Agenda Review")
                    tags.Add("Other");
                if (meetingType == "Construction Workforce Board")
                    tags.Add("Other");
                if (meetingType == "Full Commission")
                    tags.Add("Main Council Meeting");
                if (meetingType == "Board of County Commissioners")
                    tags.Add("Main Council Meeting");
                if (meetingType == "BOCC Agenda Review")
                    tags.Add("Main Council Meeting");
                if (meetingType == "Public Building Commission")
                    tags.Add("Other");
                if (meetingType == "Administration & Human Services")
                    tags.Add("Other");
                if (meetingType == "Human Rights Commission")
                    tags.Add("Other");
                if (meetingType == "Neighborhood & Community Development")
                    tags.Add("Other");
                if (meetingType == "KCTGA Comprehensive HIV Prevention & Care Plan")
                    tags.Add("Other");
                if (meetingType == "Homesteading Authority")
                    tags.Add("Other");
                if (meetingType == "Firefighters Pension System Board of Trustees")
                    tags.Add("Other");
                if (meetingType == "Fairness In Construction Board")
                    tags.Add("Other");
                if (meetingType == "Parking Policy Review Board")
                    tags.Add("Other");
                if (meetingType == "Municipal Art Commission")
                    tags.Add("Other");
                if (meetingType == "Investment Committee")
                    tags.Add("Other");
                if (meetingType == "Health Commission")
                    tags.Add("Other");
                if (meetingType == "Employees Retirement System Board of Trustees")
                    tags.Add("Other");
                if (meetingType == "Land Bank Agency")
                    tags.Add("Other");
                if (meetingType == "18th & Vine Development Policy Committee")
                    tags.Add("Other");
                if (meetingType == "Board of Police Commissioners")
                    tags.Add("Law Enforcement");
                if (meetingType == "Bicycle and Pedestrian Advisory Committee")
                    tags.Add("Other");
                if (meetingType == "Environmental Management Commission")
                    tags.Add("Other");
                if (meetingType == "Neighborhood Tourist Development Fund")
                    tags.Add("Other");
                if (meetingType == "Board of Zoning Adjustment")
                    tags.Add("Other");
                if (meetingType == "Public Improvement Advisory Committee")
                    tags.Add("Other");
                if (meetingType == "County Legislature")
                    tags.Add("County Commissioners");
                if (meetingType == "Health and Environment Committee")
                    tags.Add("Other");
                if (meetingType == "Anti-Crime Committee")
                    tags.Add("Other");
                if (meetingType.Contains("Budget"))
                    tags.Add("Finance");
                if (meetingType == "Land Use Committee")
                    tags.Add("Other");
                if (meetingType.Contains("Law Enforcement"))
                    tags.Add("Law Enforcement");
                if (meetingType == "Inter-Governmental Affairs Committee")
                    tags.Add("Other");
                if (meetingType == "City Plan Commission")
                    tags.Add("Other");
                if (meetingType == "Historic Preservation Commission")
                    tags.Add("Other");
            }
            catch
            {
                tags.Add("Other");
            }

            return tags;
        }
    }
}