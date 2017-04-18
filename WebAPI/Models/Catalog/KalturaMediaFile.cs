using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Media file details
    /// </summary>
    [Serializable]
    public class KalturaMediaFile : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [OldStandardProperty("asset_id")]
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
        /// Device types as defined in the system
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// URL of the media file to be played
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Duration of the media file
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty(PropertyName = "duration")]
        [XmlElement(ElementName = "duration")]
        public long? Duration { get; set; }

        /// <summary>
        /// External identifier for the media file
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]
        [OldStandardProperty("external_id")]
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

        /// <summary>
        /// Handling type
        /// </summary>
        [DataMember(Name = "handlingType")]
        [JsonProperty(PropertyName = "handlingType")]
        [XmlElement(ElementName = "handlingType")]
        [JsonIgnore]
        public string HandlingType { get; set; }

        /// <summary>
        /// CDN name
        /// </summary>
        [DataMember(Name = "cdnName")]
        [JsonProperty(PropertyName = "cdnName")]
        [XmlElement(ElementName = "cdnName")]
        [JsonIgnore]
        public string CdnName { get; set; }

        /// <summary>
        /// CDN code
        /// </summary>
        [DataMember(Name = "cdnCode")]
        [JsonProperty(PropertyName = "cdnCode")]
        [XmlElement(ElementName = "cdnCode")]
        [JsonIgnore]
        public string CdnCode { get; set; }

        /// <summary>
        /// Alt CDN code
        /// </summary>
        [DataMember(Name = "altCdnCode")]
        [JsonProperty(PropertyName = "altCdnCode")]
        [XmlElement(ElementName = "altCdnCode")]
        [JsonIgnore]
        public string AltCdnCode { get; set; }

        /// <summary>
        /// PPV Module 
        /// </summary>
        [DataMember(Name = "ppvModules")]
        [JsonProperty(PropertyName = "ppvModules")]
        [XmlElement(ElementName = "ppvModules")]
        [JsonIgnore]
        public KalturaStringValueArray PPVModules { get; set; }

        /// <summary>
        /// Product code 
        /// </summary>
        [DataMember(Name = "productCode")]
        [JsonProperty(PropertyName = "productCode")]
        [XmlElement(ElementName = "productCode")]
        [JsonIgnore]
        public string ProductCode { get; set; }
    }
}