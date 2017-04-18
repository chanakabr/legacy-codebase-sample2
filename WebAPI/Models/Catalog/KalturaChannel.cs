using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel details
    /// </summary>
    public class KalturaChannel : KalturaBaseChannel
    {
        /// <summary>
        /// Cannel description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

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
        public List<KalturaIntegerValue> AssetTypes { get; set; }
        
        /// <summary>
        /// Media types in the channel 
        /// -26 is EPG
        /// </summary>
        [DataMember(Name = "media_types")]
        [JsonIgnore]
        [Obsolete]
        public List<KalturaIntegerValue> MediaTypes { get; set; }

        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression")]
        [OldStandardProperty("filter_expression")]
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
        [XmlElement(ElementName = "order")]
        public KalturaAssetOrderBy Order
        {
            get;
            set;
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