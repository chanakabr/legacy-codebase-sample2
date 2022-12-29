using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Home networks
    /// </summary>
    [Serializable]
    public partial class KalturaHomeNetworkListResponse : KalturaListResponse
    {
        /// <summary>
        /// Home networks
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHomeNetwork> Objects { get; set; }
    }
}