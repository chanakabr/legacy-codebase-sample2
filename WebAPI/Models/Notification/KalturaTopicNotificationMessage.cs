using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public partial class KalturaTopicNotificationMessage : KalturaOTTObject
    {
        /// <summary>
        /// Topic notification message ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Topic notification message
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty(PropertyName = "message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Topic notification message image URL
        /// </summary>
        [DataMember(Name = "imageUrl")]
        [JsonProperty(PropertyName = "imageUrl")]
        [XmlElement(ElementName = "imageUrl")]
        public string ImageUrl { get; set; }        

        /// <summary>
        /// Topic notification ID
        /// </summary>
        [DataMember(Name = "topicNotificationId")]
        [JsonProperty(PropertyName = "topicNotificationId")]
        [XmlElement(ElementName = "topicNotificationId")]
        [SchemeProperty(MinInteger = 1)]
        public long TopicNotificationId { get; set; }

        /// <summary>
        /// Topic notification message trigger
        /// </summary>
        [DataMember(Name = "trigger")]
        [JsonProperty(PropertyName = "trigger")]
        [XmlElement(ElementName = "trigger")]
        public KalturaTrigger Trigger { get; set; }

        /// <summary>
        /// Topic notification message dispatchers
        /// </summary>
        [DataMember(Name = "dispatchers")]
        [JsonProperty(PropertyName = "dispatchers")]
        [XmlArray(ElementName = "dispatchers", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaDispatcher> Dispatchers { get; set; }

        /// <summary>
        /// Message status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAnnouncementStatus Status { get; set; }
    }

    public partial class KalturaTopicNotificationMessageListResponse : KalturaListResponse
    {
        /// <summary>
        /// Topic notification messages
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTopicNotificationMessage> Objects { get; set; }
    }
}