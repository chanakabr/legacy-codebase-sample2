using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Country details
    /// </summary>
    public class KalturaCountry : KalturaOTTObject
    {
        /// <summary>
        /// Country identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int Id { get; set; }

        /// <summary>
        /// Country name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Country code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// The main language code in the country
        /// </summary>
        [DataMember(Name = "mainLanguageCode")]
        [JsonProperty("mainLanguageCode")]
        [XmlElement(ElementName = "mainLanguageCode")]
        public string MainLanguageCode { get; set; }

        /// <summary>
        /// All the languages code that are supported in the country
        /// </summary>
        [DataMember(Name = "languagesCode")]
        [JsonProperty("languagesCode")]
        [XmlElement(ElementName = "languagesCode")]
        public string LanguagesCode { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        [DataMember(Name = "currency")]
        [JsonProperty("currency")]
        [XmlElement(ElementName = "currency")]
        public string CurrencyCode { get; set; }

        /// <summary>
        ///Currency Sign
        /// </summary>
        [DataMember(Name = "currencySign")]
        [JsonProperty("currencySign")]
        [XmlElement(ElementName = "currencySign")]
        public string CurrencySign { get; set; }

        /// <summary>
        ///Vat Percent in the country
        /// </summary>
        [DataMember(Name = "vatPercent")]
        [JsonProperty("vatPercent")]
        [XmlElement(ElementName = "vatPercent", IsNullable = true)]
        public double? VatPercent { get; set; }

    }
}