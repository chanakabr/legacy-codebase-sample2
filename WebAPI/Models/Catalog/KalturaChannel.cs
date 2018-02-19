using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel details
    /// </summary>
    public class KalturaChannel : KalturaOTTObject
    {

        private const string GENESIS_VERSION = "4.6.0.0";

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
        /// Channel system name
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName", IsNullable = true)]
        public string SystemName { get; set; }

        /// <summary>
        /// Channel description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public KalturaMultilingualString Description { get; set; }

        /// <summary>
        /// Channel images 
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        [XmlArray(ElementName = "images", IsNullable = true)]
        [XmlArrayItem("item")]
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
        [Deprecated(GENESIS_VERSION)]
        public List<KalturaIntegerValue> AssetTypes { get; set; }
        
        /// <summary>
        /// Media types in the channel 
        /// -26 is EPG
        /// </summary>
        [DataMember(Name = "media_types")]
        [JsonIgnore]
        [Obsolete]
        [Deprecated(GENESIS_VERSION)]
        public List<KalturaIntegerValue> MediaTypes { get; set; }

        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression")]
        [OldStandardProperty("filter_expression")]
        [Deprecated(GENESIS_VERSION)]
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
        [XmlElement(ElementName = "isActive")]
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
        [Deprecated(GENESIS_VERSION)]
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
        [XmlElement(ElementName = "groupBy")]
        [Deprecated(GENESIS_VERSION)]
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
        public KalturaChannelOrder OrderBy { get; set; }

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

        internal void Validate()
        {
            if (string.IsNullOrEmpty(SystemName))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (Name != null && Name.Values != null && Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            Name.Validate("multilingualName");

            if (Description != null && Description.Values != null && Description.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
            }

            if (Description != null)
            {
                Description.Validate("multilingualDescription");
            }

            OrderBy.Validate(this.GetType());
        }

        public int[] getAssetTypes()
        {
            if (AssetTypes == null && MediaTypes != null)
                AssetTypes = MediaTypes;

            if (AssetTypes == null)
                return new int[0];

            int[] assetTypes = new int[AssetTypes.Count];
            for (int i = 0; i < AssetTypes.Count; i++)
            {
                assetTypes[i] = AssetTypes[i].value;
            }

            return assetTypes;
        }

    }
}