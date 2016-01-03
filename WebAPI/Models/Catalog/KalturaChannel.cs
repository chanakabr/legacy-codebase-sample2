using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
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
        [DataMember(Name = "asset_types")]
        [JsonProperty(PropertyName = "asset_types")]
        [XmlArray(ElementName = "asset_types", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> AssetTypes { get; set; }

        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filter_expression")]
        [JsonProperty("filter_expression")]
        [XmlElement(ElementName = "filter_expression")]
        public string FilterExpression
        {
            get;
            set;
        }

    }
}