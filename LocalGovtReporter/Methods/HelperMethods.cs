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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LocalGovtReporter.Methods
{
    public static class HelperMethods
    {
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

        public static List<string> CreateTags(string meetingType)
        {
            List<string> tags = new List<string>();

            if (meetingType == "KC Parking and Transportation Commission")
                tags.Add("Transportation");
            if (meetingType == "VFKCC")
                tags.Add("Other");
            if (meetingType.Contains("Special"))
                tags.Add("Special Session");
            if (meetingType.Contains("Council"))
                tags.Add("City Council");
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
            if (meetingType == "Full Commission")
                tags.Add("County Commissioners");
            if (meetingType == "Board of County Commissioners")
                tags.Add("County Commissioners");
            if (meetingType == "BOCC Agenda Review")
                tags.Add("County Commissioners");
            if (meetingType == "Public Building Commission")
                tags.Add("Other");
            if (meetingType == "Administration & Human Services")
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

            return tags;
        }


        // Jackson County Specific
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

        // Jackson County Specific
        public static async Task ReadTableAsync(IWebDriver driver)
        {
            ReadOnlyCollection<IWebElement> tableRows = driver.FindElement(By.CssSelector(".rgMasterTable")).FindElements(By.CssSelector(".rgRow"));
            ReadOnlyCollection<IWebElement> tableAltRows = driver.FindElement(By.CssSelector(".rgMasterTable")).FindElements(By.CssSelector(".rgAltRow"));

            List<Meeting> meetingsList = new List<Meeting>();

            foreach (var tableRow in tableRows)
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
                string meetingVideo = "http://jacksonco.granicus.com/player/clip/" + meetingVideoCode;

                meetingsList.Add(new Meeting()
                {
                    SourceURL = meetingSource,
                    MeetingID = ("Jackson-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                    MeetingType = meetingType,
                    MeetingDate = meetingDate,
                    MeetingTime = meetingTime,
                    MeetingLocation = meetingLocation,
                    MeetingAddress = "415 E 12th St, Kansas City, MO 64106",
                    Latitude = "39.099123",
                    Longitude = "-94.577986",
                    Jurisdiction = "Jackson County",
                    State = "MO",
                    County = "Jackson",
                    AgendaURL = meetingAgenda,
                    MinutesURL = meetingMinutes == null ? "" : meetingMinutes,
                    VideoURL = meetingVideo == null ? "" : meetingVideo,
                    Tags = CreateTags(meetingType)
                });
            }

            foreach (var tableAltRow in tableAltRows)
            {
                string meetingType = tableAltRow.FindElements(By.TagName("td"))[0].Text.Trim();
                string meetingSource = tableAltRow.FindElements(By.TagName("td"))[0].FindElement(By.TagName("a")).GetAttribute("href");
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
                    SourceURL = meetingSource,
                    MeetingID = ("Jackson-County-" + meetingDate + "-" + meetingType).Replace(" ", "-"),
                    MeetingType = meetingType,
                    MeetingDate = meetingDate,
                    MeetingTime = meetingTime,
                    MeetingLocation = meetingLocation,
                    Jurisdiction = "Jackson County",
                    MeetingAddress = "415 E 12th St, Kansas City, MO 64106",
                    Latitude = "39.099123",
                    Longitude = "-94.577986",
                    State = "MO",
                    County = "Jackson",
                    AgendaURL = meetingAgenda,
                    MinutesURL = meetingMinutes == null ? "" : meetingMinutes,
                    VideoURL = meetingVideo == null ? "" : meetingVideo,
                    Tags = CreateTags(meetingType)
                });
            }

            await AWS.AddMeetingsAsync(AWS.GetAmazonDynamoDBClient(), meetingsList);
        }
    }
}