using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public class KalturaTopic : KalturaOTTObject
    {

        /// <summary>
        /// message id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// message
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// automaticIssueNotification
        /// </summary>
        [DataMember(Name = "automaticIssueNotification")]
        [JsonProperty("automaticIssueNotification")]
        [XmlElement(ElementName = "automaticIssueNotification")]
        public KalturaTopicAutomaticIssueNotification AutomaticIssueNotification { get; set; }

    }
}