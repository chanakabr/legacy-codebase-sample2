using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// Engagement Adapter
    /// </summary>
    public class KalturaEngagementAdapter : KalturaEngagementAdapterBase
    {
        /// <summary>
        /// Engagement adapter active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Engagement adapter adapter URL
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        public string AdapterUrl { get; set; }
               
        /// <summary>
        /// Engagement adapter extra parameters
        /// </summary>
        [DataMember(Name = "engagementAdapterSettings")]
        [JsonProperty("engagementAdapterSettings")]
        [XmlElement("engagementAdapterSettings", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }       

        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [SchemeProperty(ReadOnly = true)]
        public string SharedSecret { get; set; }
    }
}
