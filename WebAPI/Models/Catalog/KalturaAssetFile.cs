using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset file details
    /// </summary>
    [Serializable]
    public class KalturaAssetFile : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]        
        public int? AssetId { get; set; }

        /// <summary>
        /// File unique identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// Device types identifier as defined in the system
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public int? TypeId { get; set; }

        /// <summary>
        /// URL of the asset file to be played
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Duration of the asset file
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty(PropertyName = "duration")]
        [XmlElement(ElementName = "duration")]
        public long? Duration { get; set; }

        /// <summary>
        /// External identifier for the asset file
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]        
        public string ExternalId { get; set; }

        /// <summary>
        /// Billing type
        /// </summary>
        [DataMember(Name = "billingType")]
        [JsonProperty(PropertyName = "billingType")]
        [XmlElement(ElementName = "billingType")]
        [JsonIgnore]
        public string BillingType { get; set; }

        /// <summary>
        /// Quality
        /// </summary>
        [DataMember(Name = "quality")]
        [JsonProperty(PropertyName = "quality")]
        [XmlElement(ElementName = "quality")]
        [JsonIgnore]
        public string Quality { get; set; }       
    }
}