using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura Recommendation Engine Base
    /// </summary>
    public class KalturaRecommendationEngineBaseProfile : KalturaOTTObject
    {
        /// <summary>
        /// recommendation engine id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// recommendation engine name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        ///// <summary>
        ///// recommendation engine default (true/false)
        ///// </summary>
        //[DataMember(Name = "is_default")]
        //[JsonProperty("is_default")]
        //[XmlElement(ElementName = "is_default")]
        //public bool IsDefault { get; set; }
    }
}
