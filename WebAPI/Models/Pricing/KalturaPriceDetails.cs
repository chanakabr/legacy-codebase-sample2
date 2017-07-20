using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
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
        [SchemeProperty(ReadOnly = true)]
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
        [SchemeProperty(ReadOnly=true)]
        public KalturaPrice Price { get; set; }

        /// <summary>
        /// Multi currency prices for all countries and currencies
        /// </summary>
        [DataMember(Name = "multiCurrencyPrice")]
        [JsonProperty("multiCurrencyPrice")]
        [XmlElement(ElementName = "multiCurrencyPrice", IsNullable = true)]
        [SchemeProperty(RequiresPermission=(int)RequestType.WRITE)]
        public List<KalturaPrice> MultiCurrencyPrice { get; set; }

        /// <summary>
        /// A list of the descriptions for this price on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Descriptions { get; set; }
    }

    public class KalturaPriceDetailsListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of price details
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPriceDetails> Prices { get; set; }
    }
}