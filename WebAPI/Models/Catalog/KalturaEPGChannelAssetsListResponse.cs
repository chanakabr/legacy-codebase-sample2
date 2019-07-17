using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Obsolete]
    public partial class KalturaEPGChannelAssetsListResponse : KalturaListResponse
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