using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public class KalturaFollowDataBase : KalturaOTTObject
    {
        /// <summary>
        /// Announcement Id
        /// </summary>
        [DataMember(Name = "announcementId")]
        [JsonProperty(PropertyName = "announcementId")]
        [XmlElement(ElementName = "announcementId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("announcement_id")]
        public long AnnouncementId { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public int Status { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        [DataMember(Name = "title")]
        [JsonProperty(PropertyName = "title")]
        [XmlElement(ElementName = "title")]
        [SchemeProperty(ReadOnly = true)]
        public string Title { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        [DataMember(Name = "timestamp")]
        [JsonProperty(PropertyName = "timestamp")]
        [XmlElement(ElementName = "timestamp")]
        [SchemeProperty(ReadOnly = true)]
        public long Timestamp { get; set; }

        /// <summary>
        /// Follow Phrase
        /// </summary>
        [DataMember(Name = "followPhrase")]
        [JsonProperty(PropertyName = "followPhrase")]
        [XmlElement(ElementName = "followPhrase")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("follow_phrase")]
        public string FollowPhrase { get; set; }
    }
}