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
    [Obsolete]
    public class KalturaEPGChannelAssets : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetInfo> Assets { get; set; }

        /// <summary>
        /// Channel identifier
        /// </summary>
        [DataMember(Name = "channelId")]
        [JsonProperty(PropertyName = "channelId")]
        [XmlElement(ElementName = "channelId")]
        [OldStandardProperty("channel_id")]
        public int? ChannelID { get; set; }

    }
}