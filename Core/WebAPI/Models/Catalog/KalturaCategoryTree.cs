using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Category details
    /// </summary>
    [Serializable]
    public partial class KalturaCategoryTree : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the category item
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// List of category tree
        /// </summary>
        [DataMember(Name = "children")]
        [JsonProperty(PropertyName = "children")]
        [XmlElement(ElementName = "children")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaCategoryTree> Children { get; set; }

        /// <summary>
        /// List of unified Channels.
        /// </summary>
        [DataMember(Name = "unifiedChannels")]
        [JsonProperty(PropertyName = "unifiedChannels")]
        [XmlArray(ElementName = "unifiedChannels", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaUnifiedChannelInfo> UnifiedChannels { get; set; }

        /// <summary>
        /// Dynamic data
        /// </summary>
        [DataMember(Name = "dynamicData")]
        [JsonProperty("dynamicData")]
        [XmlElement(ElementName = "dynamicData", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }

        /// <summary>
        /// Category images
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        [XmlArray(ElementName = "images", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaImage> Images { get; set; }

        /// <summary>
        /// Category active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        //[SchemeProperty(RequiresPermission = (int)RequestType.READ, ReadOnly = true)] //TODO: anat
        public bool? IsActive { get; set; }

        /// <summary>
        /// Start date in seconds
        /// </summary>
        [DataMember(Name = "startDateInSeconds")]
        [JsonProperty("startDateInSeconds")]
        [XmlElement(ElementName = "startDateInSeconds", IsNullable = true)]
        [SchemeProperty(MinInteger = 0)]
        public long? StartDateInSeconds { get; set; }

        /// <summary>
        /// End date in seconds
        /// </summary>
        [DataMember(Name = "endDateInSeconds")]
        [JsonProperty("endDateInSeconds")]
        [XmlElement(ElementName = "endDateInSeconds", IsNullable = true)]
        [SchemeProperty(MinInteger = 0)]
        public long? EndDateInSeconds { get; set; }

        /// <summary>
        /// Category type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(InsertOnly = true)]
        public string Type { get; set; }

        /// <summary>
        /// Unique identifier for the category version
        /// </summary>
        [DataMember(Name = "versionId")]
        [JsonProperty(PropertyName = "versionId")]
        [XmlElement(ElementName = "versionId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? VersionId { get; set; }

        /// <summary>
        /// Virtual asset id
        /// </summary>
        [DataMember(Name = "virtualAssetId")]
        [JsonProperty("virtualAssetId")]
        [XmlElement(ElementName = "virtualAssetId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? VirtualAssetId { get; set; }

        /// <summary>
        /// Category reference identifier
        /// </summary>
        [DataMember(Name = "referenceId")]
        [JsonProperty("referenceId")]
        [XmlElement(ElementName = "referenceId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public string ReferenceId { get; set; }
    }
}