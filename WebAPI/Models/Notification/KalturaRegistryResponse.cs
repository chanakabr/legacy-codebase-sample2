using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "KalturaRegistryResponse", Namespace = "")]
    [XmlRoot("KalturaRegistryResponse")]
    public class KalturaRegistryResponse : KalturaOTTObject
    {
        /// <summary>
        /// Announcement Id
        /// </summary>
        [DataMember(Name = "announcementId")]
        [JsonProperty("announcementId")]
        [XmlElement(ElementName = "announcementId")]
        public long AnnouncementId { get; set; }

        /// <summary>
        /// Key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }
    }
}