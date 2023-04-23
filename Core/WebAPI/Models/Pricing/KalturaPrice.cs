using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Price 
    /// </summary>
    [Serializable]
    public partial class KalturaPrice : KalturaOTTObject
    {
        /// <summary>
        /// Currency ID
        /// </summary>
        [DataMember(Name = "currencyId")]
        [JsonProperty("currencyId")]
        [XmlElement(ElementName = "currencyId")]
        [SchemeProperty(ReadOnly = true, IsNullable = true)]
        public long? CurrencyId { get; set; }
        
        /// <summary>
        ///Price
        /// </summary>
        [DataMember(Name = "amount")]
        [JsonProperty("amount")]
        [XmlElement(ElementName = "amount", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MaxFloat = 99999999)] //BEO-12570
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
        [OldStandardProperty("currency_sign")]
        public string CurrencySign { get; set; }

        /// <summary>
        ///Country ID
        /// </summary>
        [DataMember(Name = "countryId")]
        [JsonProperty("countryId")]
        [XmlElement(ElementName = "countryId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, IsNullable = true)]
        public long? CountryId { get; set; }
    }
}