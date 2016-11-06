using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price details
    /// </summary>
    public class KalturaPriceDetails : KalturaOTTObject
    {
        /// <summary>
        /// The price code identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        /// <summary>
        /// The price code name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string name { get; set; }

        /// <summary>
        /// The price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        public KalturaPrice Price { get; set; }

        /// <summary>
        /// A list of the descriptions for this price on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Descriptions { get; set; }
    }
}