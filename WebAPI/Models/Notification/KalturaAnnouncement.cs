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
    public class KalturaAnnouncement : KalturaOTTObject
    {
        /// <summary>
        /// Announcement name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Announcement message
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty(PropertyName = "message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Announcement enabled
        /// </summary>
        [DataMember(Name = "enabled")]
        [JsonProperty(PropertyName = "enabled")]
        [XmlElement(ElementName = "enabled")]
        public bool? Enabled { get; set; }

        /// <summary>
        /// Announcement start time
        /// </summary>
        [DataMember(Name = "startTime")]
        [JsonProperty(PropertyName = "startTime")]
        [XmlElement(ElementName = "startTime")]
        [OldStandardProperty("start_time")]
        public long? StartTime { get; set; }

        /// <summary>
        /// Announcement time zone
        /// </summary>
        [DataMember(Name = "timezone")]
        [JsonProperty(PropertyName = "timezone")]
        [XmlElement(ElementName = "timezone")]
        public string Timezone { get; set; }

        /// <summary>
        /// Announcement status: NotSent=0/Sending=1/Sent=2/Aborted=3
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAnnouncementStatus Status { get; set; }

        /// <summary>
        /// Announcement recipients: All=0/LoggedIn=1/Guests=2/Other=3
        /// </summary>
        [DataMember(Name = "recipients")]
        [JsonProperty(PropertyName = "recipients")]
        [XmlElement(ElementName = "recipients")]
        public KalturaAnnouncementRecipientsType Recipients { get; set; }

        /// <summary>
        /// Announcement id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        internal long getStartTime()
        {
            return StartTime.HasValue ? (long)StartTime : 0;
        }

        internal int getId()
        {
            return Id.HasValue ? (int)Id : 0;
        }

        internal bool getEnabled()
        {
            return Enabled.HasValue ? (bool)Enabled : true;
        }
    }
}