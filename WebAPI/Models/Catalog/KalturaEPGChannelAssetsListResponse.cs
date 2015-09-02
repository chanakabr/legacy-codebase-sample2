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
    public class KalturaEPGChannelAssetsListResponse : KalturaListResponse
    {
        /// <summary>
        /// Channels
        /// </summary>
        [DataMember(Name = "assets")]
        [JsonProperty(PropertyName = "assets")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem("item")]
        public List<KalturaEPGChannelAssets> Channels { get; set; }
    }
}