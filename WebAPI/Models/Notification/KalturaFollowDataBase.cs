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
    [OldStandard("announcementId", "announcement_id")]
    [OldStandard("followPhrase", "follow_phrase")]
    public class KalturaFollowDataBase : KalturaOTTObject
    {
        /// <summary>
        /// Announcement Id
        /// </summary>
        [DataMember(Name = "announcementId")]
        [JsonProperty(PropertyName = "announcementId")]
        [XmlElement(ElementName = "announcementId")]
        public long AnnouncementId { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        public int Status { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        [DataMember(Name = "title")]
        [JsonProperty(PropertyName = "title")]
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        [DataMember(Name = "timestamp")]
        [JsonProperty(PropertyName = "timestamp")]
        [XmlElement(ElementName = "timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Follow Phrase
        /// </summary>
        [DataMember(Name = "followPhrase")]
        [JsonProperty(PropertyName = "followPhrase")]
        [XmlElement(ElementName = "followPhrase")]
        public string FollowPhrase { get; set; }
    }
}