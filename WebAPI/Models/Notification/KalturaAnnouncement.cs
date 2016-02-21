using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public class KalturaAnnouncement : KalturaOTTObject
    {
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [DataMember(Name = "message")]
        [JsonProperty(PropertyName = "message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [DataMember(Name = "enabled")]
        [JsonProperty(PropertyName = "enabled")]
        [XmlElement(ElementName = "enabled")]
        public bool Enabled { get; set; }

        [DataMember(Name = "start_time")]
        [JsonProperty(PropertyName = "start_time")]
        [XmlElement(ElementName = "start_time")]
        public long StartTime { get; set; }

        [DataMember(Name = "timezone")]
        [JsonProperty(PropertyName = "timezone")]
        [XmlElement(ElementName = "timezone")]
        public string Timezone { get; set; }

        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        public KalturaAnnouncementStatus Status { get; set; }

        [DataMember(Name = "recipients")]
        [JsonProperty(PropertyName = "recipients")]
        [XmlElement(ElementName = "recipients")]
        public KalturaAnnouncementRecipientsType Recipients { get; set; }

        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }
    }
}