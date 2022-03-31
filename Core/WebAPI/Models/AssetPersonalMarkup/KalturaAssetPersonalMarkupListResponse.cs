using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.AssetPersonalMarkup
{
    /// <summary>
    /// Asset Personal Markup
    /// </summary>
    [Serializable]
    public partial class KalturaAssetPersonalMarkupListResponse : KalturaListResponse
    {
        /// <summary>
        /// Adapters
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetPersonalMarkup> Objects { get; set; }
    }
}