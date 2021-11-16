using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using LocalGovtReporter.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace LocalGovtReporter.Methods
{
    public class AWS
    {
        //public static AmazonDynamoDBClient GetAmazonDynamoDBClient()
        //{
        //    var credentials = new BasicAWSCredentials("accessKey", "secretKey");
        //    var amazonDynamoDBClient = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
        //    return amazonDynamoDBClient;
        //}
        public static AmazonDynamoDBClient GetAmazonDynamoDBClient()
        {
            AmazonDynamoDBClient client = null;

#if DEBUG
            var sharedFile = new SharedCredentialsFile();
            sharedFile.TryGetProfile("localgovt", out var profile);
            AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);
            client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
#else
                client = new AmazonDynamoDBClient(RegionEndpoint.USEast2);
#endif

            return client;
        }
        public static async Task AddMeetingsAsync(AmazonDynamoDBClient client, List<Meeting> meetings, string agency)
        {
            Methods.HelperMethods.MessageAddingMeetingList(agency, meetings.Count);
            int count = 0;
            foreach (var meeting in meetings)
            {
                count++;
                Console.WriteLine($"Adding meeting {count} of {meetings.Count}");

                var dictionary = new Dictionary<string, AttributeValue>();
                try
                {

                    foreach (PropertyInfo prop in meeting.GetType().GetProperties())
                    {
                        object value = prop.GetValue(meeting, null);

      //                  if (value != null)
						//{
						//	if (prop.Name == "SourceURL" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("SourceURL", new AttributeValue { S = meeting.SourceURL });
						//	if (prop.Name == "MeetingID" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("MeetingID", new AttributeValue { S = meeting.MeetingID });
						//	if (prop.Name == "MeetingType" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("MeetingType", new AttributeValue { S = meeting.MeetingType });
						//	if (prop.Name == "Jurisdiction" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("Jurisdiction", new AttributeValue { S = meeting.Jurisdiction });
						//	if (prop.Name == "State" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("State", new AttributeValue { S = meeting.State });
						//	if (prop.Name == "County" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("County", new AttributeValue { S = meeting.County });
						//	if (prop.Name == "MeetingDate" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("MeetingDate", new AttributeValue { S = meeting.MeetingDate });
						//	if (prop.Name == "MeetingLocation" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("MeetingLocation", new AttributeValue { S = meeting.MeetingLocation });
						//	if (prop.Name == "AgendaURL" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("AgendaURL", new AttributeValue { S = meeting.AgendaURL });
						//	if (prop.Name == "PacketURL" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("PacketURL", new AttributeValue { S = meeting.PacketURL });
						//	if (prop.Name == "VideoURL" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("VideoURL", new AttributeValue { S = meeting.VideoURL });
						//	if (prop.Name == "MinutesURL" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("MinutesURL", new AttributeValue { S = meeting.MinutesURL });
						//	if (prop.Name == "Tags" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("Tags", new AttributeValue { SS = meeting.Tags });
						//	if (prop.Name == "MeetingAddress" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("MeetingAddress", new AttributeValue { S = meeting.MeetingAddress });
						//	if (prop.Name == "Latitude" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("Latitude", new AttributeValue { S = meeting.Latitude });
						//	if (prop.Name == "Longitude" && !string.IsNullOrEmpty(value.ToString()))
						//		dictionary.Add("Longitude", new AttributeValue { S = meeting.Longitude });
						//}
                        AddToDictionary(meeting, dictionary, prop.Name, value);
                    }
                    await client.PutItemAsync(new PutItemRequest
                    {
                        TableName = "Meeting",
                        Item = dictionary
                    });
                }
                catch (Exception ex)
                {
                    foreach (KeyValuePair<string, AttributeValue> kvp in dictionary)
                    {
                        if (kvp.Key.Equals("MeetingID") || kvp.Key.Equals("MeetingDate"))
                            Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value.S);
                    }
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private static List<string> properties = new List<string>(){ "SourceURL", "MeetingID", "MeetingType" ,
            "Jurisdiction", "State", "County" ,"MeetingDate", "MeetingLocation", "AgendaURL"
            ,"PacketURL", "VideoURL", "MinutesURL","Tags", "MeetingAddress", "Latitude", "Longitude" };
        
        private static void AddToDictionary(Meeting meeting, Dictionary<string, AttributeValue> dictionary, string propertyName, object value)
		{
            bool propertyWeTrack = properties.Any(s => s.Contains(propertyName));
            if (propertyWeTrack)
            {
                string propValue = "";
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    propValue = value.ToString();
                    if (propertyName.Equals("Tags"))
                    {
                        List<string> tags = (List<string>)value;
                        if (tags.Count > 0)
                            dictionary.Add(propertyName, new AttributeValue { SS = (List<string>)value });
                    }
                    else
                        dictionary.Add(propertyName, new AttributeValue { S = propValue });
                }
                else
                {
                    //if (propertyName.Equals("Tags"))
                    //{
                    //    dictionary.Add(propertyName, new AttributeValue { SS = new List<string>() });
                    //}
                    //else
                    //{
                    //    dictionary.Add(propertyName, new AttributeValue { S = propValue });
                    //}
                }
            }
		}
	}
}