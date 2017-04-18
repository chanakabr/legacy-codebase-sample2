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
    public class KalturaEPGChannelAssetsListResponse : KalturaListResponse
    {
        /// <summary>
        /// Channels
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("assets")]
        public List<KalturaEPGChannelAssets> Channels { get; set; }
    }
}