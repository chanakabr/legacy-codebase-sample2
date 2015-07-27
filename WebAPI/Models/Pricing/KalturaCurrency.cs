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
    /// Currency
    /// </summary>
    [Serializable]
    public class KalturaCurrency : KalturaOTTObject
    {
        /// <summary>
        ///CurrencyCD3
        /// </summary>
        [DataMember(Name = "currency_cd3")]
        [JsonProperty("currency_cd3")]
        [XmlElement(ElementName = "currency_cd3")]
        public string currencyCD3 { get; set; }

        /// <summary>
        ///currencyCD2
        /// </summary>
        [DataMember(Name = "currency_cd2")]
        [JsonProperty("currency_cd2")]
        [XmlElement(ElementName = "currency_cd2")]
        public string currencyCD2;

        /// <summary>
        ///Currency Sign
        /// </summary>
        [DataMember(Name = "currency_sign")]
        [JsonProperty("currency_sign")]
        [XmlElement(ElementName = "currency_sign")]
        public string currencySign;

        /// <summary>
        ///Currency ID
        /// </summary>
        [DataMember(Name = "currency_id")]
        [JsonProperty("currency_id")]
        [XmlElement(ElementName = "currency_id")]
        public Int32 currencyID;
    }
}