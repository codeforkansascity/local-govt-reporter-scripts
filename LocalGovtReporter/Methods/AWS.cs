using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using LocalGovtReporter.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace LocalGovtReporter.Methods
{
    public class AWS
    {
        public static AmazonDynamoDBClient GetAmazonDynamoDBClient()
        {
            var credentials = new BasicAWSCredentials("accessKey", "secretKey");
            var amazonDynamoDBClient = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast2);
            return amazonDynamoDBClient;
        }

        public static async Task AddMeetingsAsync(AmazonDynamoDBClient client, List<Meeting> meetings)
        {            
            foreach (var meeting in meetings)
            {
                var dictionary = new Dictionary<string, AttributeValue>();

                foreach (PropertyInfo prop in meeting.GetType().GetProperties())
                {
                    object value = prop.GetValue(meeting, null);

                    if (value != null)
                    {
                        if (prop.Name == "SourceURL" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("SourceURL", new AttributeValue { S = meeting.SourceURL });
                        if (prop.Name == "MeetingID" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("MeetingID", new AttributeValue { S = meeting.MeetingID });
                        if (prop.Name == "MeetingType" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("MeetingType", new AttributeValue { S = meeting.MeetingType });
                        if (prop.Name == "Jurisdiction" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("Jurisdiction", new AttributeValue { S = meeting.Jurisdiction });
                        if (prop.Name == "State" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("State", new AttributeValue { S = meeting.State });
                        if (prop.Name == "County" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("County", new AttributeValue { S = meeting.County });
                        if (prop.Name == "MeetingDate" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("MeetingDate", new AttributeValue { S = meeting.MeetingDate });
                        if (prop.Name == "MeetingLocation" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("MeetingLocation", new AttributeValue { S = meeting.MeetingLocation });
                        if (prop.Name == "AgendaURL" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("AgendaURL", new AttributeValue { S = meeting.AgendaURL });
                        if (prop.Name == "PacketURL" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("PacketURL", new AttributeValue { S = meeting.PacketURL });
                        if (prop.Name == "VideoURL" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("VideoURL", new AttributeValue { S = meeting.VideoURL });
                        if (prop.Name == "MinutesURL" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("MinutesURL", new AttributeValue { S = meeting.MinutesURL });
                        if (prop.Name == "Tags" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("Tags", new AttributeValue { SS = meeting.Tags });
                        if (prop.Name == "MeetingAddress" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("MeetingAddress", new AttributeValue { S = meeting.MeetingAddress });
                        if (prop.Name == "Latitude" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("Latitude", new AttributeValue { S = meeting.Latitude });
                        if (prop.Name == "Longitude" && !string.IsNullOrEmpty(value.ToString()))
                            dictionary.Add("Longitude", new AttributeValue { S = meeting.Longitude });
                    }
                }

                await client.PutItemAsync(new PutItemRequest
                {
                    TableName = "Meeting",
                    Item = dictionary
                });
            }
        }
    }
}