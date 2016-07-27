using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// KSQL Channel
    /// </summary>
    [OldStandard("isActive", "is_active")]
    [OldStandard("filterExpression", "filter_expression")]
    [OldStandard("assetTypes", "asset_types")]
    [Obsolete]
    public class KalturaChannelProfile : KalturaOTTObject
    {
        /// <summary>
        /// Channel id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int? Id
        {
            get;
            set;
        }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description
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
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression")]
        public string FilterExpression
        {
            get;
            set;
        }

        /// <summary>
        /// Asset types. Media types - taken from group's definition. EPG is -26.
        /// </summary>
        [DataMember(Name = "assetTypes")]
        [JsonProperty("assetTypes")]
        [XmlArray(ElementName = "assetTypes", IsNullable = true)]
        [XmlArrayItem("assetTypes")]
        public List<int> AssetTypes
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
        public KalturaOrder Order
        {
            get;
            set;
        }
    }
}