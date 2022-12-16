using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public partial class KalturaTopicNotificationMessageFilter : KalturaFilter<KalturaTopicNotificationMessageOrderBy>
    {
        /// <summary>
        /// Topic notification ID
        /// </summary>
        [DataMember(Name = "topicNotificationIdEqual")]
        [JsonProperty(PropertyName = "topicNotificationIdEqual")]
        [XmlElement(ElementName = "topicNotificationIdEqual")]
        public long TopicNotificationIdEqual { get; set; }

        public override KalturaTopicNotificationMessageOrderBy GetDefaultOrderByValue()
        {
            return KalturaTopicNotificationMessageOrderBy.NONE;
        }
    }
}