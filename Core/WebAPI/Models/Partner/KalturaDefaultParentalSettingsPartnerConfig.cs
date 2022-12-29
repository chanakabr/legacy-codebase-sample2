using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Partner
{
    public partial class KalturaDefaultParentalSettingsPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// defaultTvSeriesParentalRuleId
        /// </summary>
        [DataMember(Name = "defaultMoviesParentalRuleId")]
        [JsonProperty("defaultMoviesParentalRuleId")]
        [XmlElement(ElementName = "defaultMoviesParentalRuleId")]
        [SchemeProperty(IsNullable = true)]
        public long? DefaultMoviesParentalRuleId { get; set; }

        /// <summary>
        /// defaultTvSeriesParentalRuleId
        /// </summary>
        [DataMember(Name = "defaultTvSeriesParentalRuleId")]
        [JsonProperty("defaultTvSeriesParentalRuleId")]
        [XmlElement(ElementName = "defaultTvSeriesParentalRuleId")]
        [SchemeProperty(IsNullable = true)]
        public long? DefaultTvSeriesParentalRuleId { get; set; }

        /// <summary>
        /// defaultParentalPin
        /// </summary>
        [DataMember(Name = "defaultParentalPin")]
        [JsonProperty("defaultParentalPin")]
        [XmlElement(ElementName = "defaultParentalPin")]
        [SchemeProperty(IsNullable = true, MaxLength = 50)]
        public string DefaultParentalPin{ get; set; }

        /// <summary>
        /// defaultPurchasePin
        /// </summary>
        [DataMember(Name = "defaultPurchasePin")]
        [JsonProperty("defaultPurchasePin")]
        [XmlElement(ElementName = "defaultPurchasePin")]
        [SchemeProperty(IsNullable = true, MaxLength = 50)]
        public string DefaultPurchasePin { get; set; }

        /// <summary>
        /// defaultPurchaseSettings
        /// </summary>
        [DataMember(Name = "defaultPurchaseSettings")]
        [JsonProperty("defaultPurchaseSettings")]
        [XmlElement(ElementName = "defaultPurchaseSettings")]
        [SchemeProperty(IsNullable = true)]
        public long? DefaultPurchaseSettings { get; set; }
    }
}
