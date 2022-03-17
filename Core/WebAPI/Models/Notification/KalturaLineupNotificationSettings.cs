using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public partial class KalturaLineupNotificationSettings : KalturaOTTObject
    {
        /// <summary>
        /// <see langword="true"/> if lineup notifications are enabled.
        /// </summary>
        [DataMember(Name = "enabled")]
        [JsonProperty("enabled")]
        [XmlElement(ElementName = "enabled")]
        public bool Enabled { get; set; }
    }
}