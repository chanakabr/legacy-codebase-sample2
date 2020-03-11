using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Identifier filter 
    /// </summary>
    [Obsolete]
    public partial class KalturaIdentifierTypeFilter : KalturaOTTObject
    {
        /// <summary>
        ///The identifier
        /// </summary>
        [DataMember(Name = "identifier")]
        [JsonProperty("identifier")]
        [XmlElement(ElementName = "identifier")]
        public string Identifier { get; set; }

        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaIdentifierTypeBy By { get; set; }
    }
}