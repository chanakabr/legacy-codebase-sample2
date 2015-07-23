using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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
        [DataMember(Name = "currencycd3")]
        [JsonProperty("currencycd3")]
        public string currencyCD3 { get; set; }

        /// <summary>
        ///currencyCD2
        /// </summary>
        [DataMember(Name = "currencycd2")]
        [JsonProperty("currencycd2")]
        public string currencyCD2;

        /// <summary>
        ///Currency Sign
        /// </summary>
        [DataMember(Name = "currencysign")]
        [JsonProperty("currencysign")]
        public string currencySign;

        /// <summary>
        ///Currency ID
        /// </summary>
        [DataMember(Name = "currencyid")]
        [JsonProperty("currencyid")]
        public Int32 currencyID;
    }
}