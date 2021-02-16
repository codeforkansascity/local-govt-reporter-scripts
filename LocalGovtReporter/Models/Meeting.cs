using System.Collections.Generic;

namespace LocalGovtReporter.Models
{
    public class Meeting
    {
        public string SourceURL { get; set; }
        public string MeetingID { get; set; }
        public string MeetingType { get; set; }
        public string MeetingDate { get; set; }
        public string MeetingTime { get; set; }
        public string MeetingLocation { get; set; }
        public string Jurisdiction { get; set; }
        public string State { get; set; }
        public string County { get; set; }
        public string AgendaURL { get; set; }
        public string PacketURL { get; set; }
        public string MinutesURL { get; set; }
        public string VideoURL { get; set; }
        public List<string> Tags { get; set; }
    }
}