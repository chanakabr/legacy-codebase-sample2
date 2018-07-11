using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
namespace WebAPI.Models.API
{
    /// <summary>
    /// Media concurrency rule 
    /// </summary>
    public class KalturaMediaConcurrencyRule : KalturaOTTObject
    {
        /// <summary>
        ///  Media concurrency rule  identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        ///  Media concurrency rule  name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Concurrency limitation type
        /// </summary>
        [DataMember(Name = "concurrencyLimitationType")]
        [JsonProperty("concurrencyLimitationType")]
        [XmlElement(ElementName = "concurrencyLimitationType")]
        public KalturaConcurrencyLimitationType ConcurrencyLimitationType { get; set; }

        /// <summary>
        /// Limitation
        /// </summary>
        [DataMember(Name = "limitation")]
        [JsonProperty("limitation")]
        [XmlElement(ElementName = "limitation")]
        public int Limitation { get; set; }
    }
}