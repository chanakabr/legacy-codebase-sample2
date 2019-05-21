using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
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

    public enum KalturaTopicNotificationMessageOrderBy
    {
        NONE
    }

}