using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public enum KalturaTopicOrderBy
    {
        NONE
    }

    public class KalturaTopicFilter : KalturaFilter<KalturaTopicOrderBy>
    {
        public override KalturaTopicOrderBy GetDefaultOrderByValue()
        {
            return KalturaTopicOrderBy.NONE;
        }
    }

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
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// message
        /// </summary>
        [DataMember(Name = "subscribersAmount")]
        [JsonProperty("subscribersAmount")]
        [XmlElement(ElementName = "subscribersAmount")]
        public string SubscribersAmount { get; set; }

        /// <summary>
        /// automaticIssueNotification
        /// </summary>
        [DataMember(Name = "automaticIssueNotification")]
        [JsonProperty("automaticIssueNotification")]
        [XmlElement(ElementName = "automaticIssueNotification")]
        public KalturaTopicAutomaticIssueNotification AutomaticIssueNotification { get; set; }

        /// <summary>
        /// lastMessageSentDateSec
        /// </summary>
        [DataMember(Name = "lastMessageSentDateSec")]
        [JsonProperty("lastMessageSentDateSec")]
        [XmlElement(ElementName = "lastMessageSentDateSec")]
        public long LastMessageSentDateSec { get; set; }

    }
}