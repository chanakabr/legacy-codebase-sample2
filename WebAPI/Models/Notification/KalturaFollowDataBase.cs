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
    public abstract class KalturaFollowDataBase : KalturaOTTObject
    {
        // TODO SHIR - ASK TAN TAN ABOUT THE NAME (ID - WE DO IT Deprecated(4.9) OR AS IS)
        /// <summary>
        /// Announcement Id
        /// </summary>
        [DataMember(Name = "announcementId")]
        [JsonProperty(PropertyName = "announcementId")]
        [XmlElement(ElementName = "announcementId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("announcement_id")]
        public long AnnouncementId { get; set; }

        // TODO SHIR - CHECK IF NOT RETURN 2 FROM DB SO WE Deprecated(4.9) IT + OR AS IS
        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public int Status { get; set; }

        // TODO SHIR - ASK TAN TAN ABOUT THE NAME (NAME - WE DO IT Deprecated(4.9) + STOP READONLY OR AS IS)
        /// <summary>
        /// Title
        /// </summary>
        [DataMember(Name = "title")]
        [JsonProperty(PropertyName = "title")]
        [XmlElement(ElementName = "title")]
        [SchemeProperty(ReadOnly = true)]
        public string Title { get; set; }

        // TODO SHIR - ASK TAN TAN ABOUT THE NAME (CREATE_DATE - WE DO IT Deprecated(4.9) OR AS IS)
        /// <summary>
        /// Timestamp
        /// </summary>
        [DataMember(Name = "timestamp")]
        [JsonProperty(PropertyName = "timestamp")]
        [XmlElement(ElementName = "timestamp")]
        [SchemeProperty(ReadOnly = true)]
        public long Timestamp { get; set; }

        // TODO SHIR - ASK TAN TAN ABOUT THE NAME (KSQL - WE DO IT Deprecated(4.9) OR AS IS)
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