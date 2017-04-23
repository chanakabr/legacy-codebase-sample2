using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// Engagement
    /// </summary>
    public class KalturaEngagement : KalturaOTTObject
    {
        /// <summary>
        /// Engagement id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int Id { get; set; }

        /// <summary>
        /// Total number of recipients
        /// </summary>
        [DataMember(Name = "totalNumberOfRecipients")]
        [JsonProperty("totalNumberOfRecipients")]
        [XmlElement(ElementName = "totalNumberOfRecipients")]
        [SchemeProperty(ReadOnly = true)]
        public int TotalNumberOfRecipients { get; set; }

        /// <summary>
        /// Engagement type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaEngagementType Type { get; set; }

        /// <summary>
        /// Engagement adapter id
        /// </summary>
        [DataMember(Name = "adapterId")]
        [JsonProperty("adapterId")]
        [XmlElement(ElementName = "adapterId")]
        public int AdapterId { get; set; }

        /// <summary>
        /// Engagement adapter dynamic data
        /// </summary>
        [DataMember(Name = "adapterDynamicData")]
        [JsonProperty("adapterDynamicData")]
        [XmlElement(ElementName = "adapterDynamicData")]        
        public string AdapterDynamicData { get; set; }

        /// <summary>
        /// Interval (seconds)
        /// </summary>
        [DataMember(Name = "intervalSeconds")]
        [JsonProperty("intervalSeconds")]
        [XmlElement(ElementName = "intervalSeconds")]        
        public int IntervalSeconds { get; set; }

        /// <summary>
        /// Manual User list
        /// </summary>
        [DataMember(Name = "userList")]
        [JsonProperty("userList")]
        [XmlElement(ElementName = "userList")]
        public string UserList { get; set; }

        /// <summary>
        /// Send time (seconds)
        /// </summary>
        [DataMember(Name = "sendTimeInSeconds")]
        [JsonProperty("sendTimeInSeconds")]
        [XmlElement(ElementName = "sendTimeInSeconds")]
        public long SendTimeInSeconds { get; set; }
    }
}
