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
    public class KalturaEPGChannelAssets : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem("item")]
        public List<KalturaAssetInfo> Assets { get; set; }

        /// <summary>
        /// Channel identifier
        /// </summary>
        [DataMember(Name = "channel_id")]
        [JsonProperty(PropertyName = "channel_id")]
        [XmlElement(ElementName = "channel_id")]
        public int ChannelID { get; set; }

    }
}