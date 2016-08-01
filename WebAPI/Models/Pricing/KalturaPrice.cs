using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price 
    /// </summary>
    [Serializable]
    [OldStandard("currencySign", "currency_sign")]
    public class KalturaPrice : KalturaOTTObject
    {
        /// <summary>
        ///Price
        /// </summary>
        [DataMember(Name = "amount")]
        [JsonProperty("amount")]
        [XmlElement(ElementName = "amount")]
        public double? Amount { get; set; }

        /// <summary>
        ///Currency
        /// </summary>
        [DataMember(Name = "currency")]
        [JsonProperty("currency")]
        [XmlElement(ElementName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        ///Currency Sign
        /// </summary>
        [DataMember(Name = "currencySign")]
        [JsonProperty("currencySign")]
        [XmlElement(ElementName = "currencySign")]
        public string CurrencySign { get; set; }
    }
}