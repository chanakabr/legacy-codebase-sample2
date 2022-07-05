using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog.Ordering;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel details
    /// </summary>
    public partial class KalturaChannel : KalturaBaseChannel
    {
        /// <summary>
        /// Unique identifier for the channel
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "oldName")]
        [JsonProperty(PropertyName = "oldName")]
        [XmlElement(ElementName = "oldName")]
        [OldStandardProperty("name", "5.0.0.0")]
        public string OldName { get; set; }

        /// <summary>
        /// Channel system name
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName", IsNullable = true)]
        public string SystemName { get; set; }

        /// <summary>
        /// Cannel description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public KalturaMultilingualString Description { get; set; }

        /// <summary>
        /// Cannel description
        /// </summary>
        [DataMember(Name = "oldDescription")]
        [JsonProperty(PropertyName = "oldDescription")]
        [XmlElement(ElementName = "oldDescription")]
        [OldStandardProperty("description", "5.0.0.0")]
        public string OldDescription { get; set; }

        /// <summary>
        /// Channel images 
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        [XmlArray(ElementName = "images", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        [XmlArrayItem("item")]
        [Deprecated("5.0.0.0")]
        public List<KalturaMediaImage> Images { get; set; }

        /// <summary>
        /// Asset types in the channel.
        /// -26 is EPG
        /// </summary>
        [DataMember(Name = "assetTypes")]
        [JsonProperty(PropertyName = "assetTypes")]
        [XmlArray(ElementName = "assetTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("asset_types")]
        [Deprecated("5.0.0.0")]
        [Obsolete]
        public List<KalturaIntegerValue> AssetTypes { get; set; }

        /// <summary>
        /// Media types in the channel 
        /// -26 is EPG
        /// </summary>
        [DataMember(Name = "media_types")]
        [JsonIgnore]
        [Obsolete]
        [Deprecated("5.0.0.0")]
        public List<KalturaIntegerValue> MediaTypes { get; set; }

        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression")]
        [OldStandardProperty("filter_expression")]
        [Deprecated("5.0.0.0")]
        public string FilterExpression
        {
            get;
            set;
        }

        /// <summary>
        /// active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? IsActive
        {
            get;
            set;
        }

        /// <summary>
        /// Channel order
        /// </summary>
        [DataMember(Name = "order")]
        [JsonProperty("order")]
        [XmlElement(ElementName = "order", IsNullable = true)]
        [Deprecated("5.0.0.0")]
        public KalturaAssetOrderBy? Order
        {
            get;
            set;
        }
        
        /// <summary>
        /// Channel group by
        /// </summary>
        [DataMember(Name = "groupBy")]
        [JsonProperty("groupBy")]
        [XmlElement(ElementName = "groupBy", IsNullable = true)]
        [Deprecated("5.0.0.0")]
        [Obsolete]
        public KalturaAssetGroupBy GroupBy
        {
            get;
            set;
        }

        /// <summary>
        /// Channel order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaChannelOrder OrderBy { get; set; }

        /// <summary>
        /// Parameters for asset list sorting.
        /// </summary>
        [DataMember(Name = "orderingParametersEqual")]
        [JsonProperty(PropertyName = "orderingParametersEqual")]
        [XmlElement(ElementName = "orderingParametersEqual")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public List<KalturaBaseChannelOrder> OrderingParameters { get; set; }

        /// <summary>
        /// Specifies when was the Channel was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Channel last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Specifies whether the assets in this channel will be ordered based on their match to the user's segments (see BEO-5524)
        /// </summary>
        [DataMember(Name = "supportSegmentBasedOrdering")]
        [JsonProperty("supportSegmentBasedOrdering")]
        [XmlElement(ElementName = "supportSegmentBasedOrdering")]
        [SchemeProperty()]
        public bool SupportSegmentBasedOrdering { get; set; }

        /// <summary>
        /// Asset user rule identifier 
        /// </summary>
        [DataMember(Name = "assetUserRuleId")]
        [JsonProperty("assetUserRuleId")]
        [XmlElement(ElementName = "assetUserRuleId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE, IsNullable = true)]
        public long? AssetUserRuleId { get; set; }

        /// <summary>
        /// key/value map field for extra data
        /// </summary>
        [DataMember(Name = "metaData")]
        [JsonProperty("metaData")]
        [XmlElement(ElementName = "metaData", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> MetaData { get; set; }

        /// <summary>
        /// Virtual asset id
        /// </summary>
        [DataMember(Name = "virtualAssetId")]
        [JsonProperty("virtualAssetId")]
        [XmlElement(ElementName = "virtualAssetId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? VirtualAssetId { get; set; }
    }
}