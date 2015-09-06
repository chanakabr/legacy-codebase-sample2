using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// PaymentGW
    /// </summary>
    public class KalturaOSSAdapterProfile : KalturaOSSAdapterBaseProfile
    {


        /// <summary>
        /// Payment gateway is active status
        /// </summary>
        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        [XmlElement(ElementName = "is_active")]
        public int IsActive { get; set; }

        /// <summary>
        /// Payment gateway adapter URL
        /// </summary>
        [DataMember(Name = "adapter_url")]
        [JsonProperty("adapter_url")]
        [XmlElement(ElementName = "adapter_url")]
        public string AdapterUrl { get; set; }
               
        /// <summary>
        /// OSS adapter extra parameters
        /// </summary>
        [DataMember(Name = "oss_adapter_settings")]
        [JsonProperty("oss_adapter_settings")]
        [XmlElement(ElementName = "oss_adapter_settings")]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// Payment gateway external identifier
        /// </summary>
        [DataMember(Name = "external_identifier")]
        [JsonProperty("external_identifier")]
        [XmlElement(ElementName = "external_identifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "shared_secret")]
        [JsonProperty("shared_secret")]
        [XmlElement(ElementName = "shared_secret")]
        public string SharedSecret { get; set; }
    }
}
