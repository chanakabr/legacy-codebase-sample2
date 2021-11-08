using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public partial class KalturaCategoryVersionFilterByTree : KalturaCategoryVersionFilter
    {
        /// <summary>
        /// Category version tree identifier
        /// </summary>
        [DataMember(Name = "treeIdEqual")]
        [JsonProperty("treeIdEqual")]
        [XmlElement(ElementName = "treeIdEqual")]
        [SchemeProperty(MinLong = 1)]
        public long TreeIdEqual { get; set; }

        /// <summary>
        /// Category version state
        /// </summary>
        [DataMember(Name = "stateEqual")]
        [JsonProperty("stateEqual")]
        [XmlElement(ElementName = "stateEqual", IsNullable = true)]
        public KalturaCategoryVersionState? StateEqual { get; set; }
    }
}