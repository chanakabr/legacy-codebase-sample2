using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Result of action performed on entity with Id
    /// </summary>
    [Serializable]
    public partial class KalturaActionResult : KalturaOTTObject
    {
        /// <summary>
        /// Identifier of entity
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id", IsNullable = false)]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Identifier of entity
        /// </summary>
        [DataMember(Name = "result")]
        [JsonProperty("result")]
        [XmlElement(ElementName = "result", IsNullable = false)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaMessage Result { get; set; }
    }
}