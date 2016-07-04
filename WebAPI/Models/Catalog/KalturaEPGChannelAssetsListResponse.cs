using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [OldStandard("objects", "assets")]
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
        public List<KalturaEPGChannelAssets> Channels { get; set; }
    }
}