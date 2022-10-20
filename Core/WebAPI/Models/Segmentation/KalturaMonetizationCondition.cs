using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Defines a condition which is essentially a combination of several monetization-based actions, each has their own score multiplier
    /// </summary>
    [SchemeClass(Required = new[] { "days", "type", "operator", "currencyCode" }, AnyOf = new[] { "minValue", "maxValue" })]
    public partial class KalturaMonetizationCondition : KalturaBaseSegmentCondition
    {
        /// <summary>
        /// The minimum value to be met
        /// </summary>
        [DataMember(Name = "minValue")]
        [JsonProperty(PropertyName = "minValue")]
        [XmlElement(ElementName = "minValue")]
        [SchemeProperty(IsNullable = true, MinInteger = 0)]
        public int? MinValue { get; set; }

        /// <summary>
        /// The maximum value to be met
        /// </summary>
        [DataMember(Name = "maxValue")]
        [JsonProperty(PropertyName = "maxValue")]
        [XmlElement(ElementName = "maxValue")]
        [SchemeProperty(IsNullable = true, MinInteger = 0)]
        public int? MaxValue { get; set; }

        /// <summary>
        /// How many days back should the actions be considered
        /// </summary>
        [DataMember(Name = "days")]
        [JsonProperty(PropertyName = "days")]
        [XmlElement(ElementName = "days")]
        [SchemeProperty(MinInteger = 0, MaxInteger = 700)]
        public int Days { get; set; }

        /// <summary>
        /// Purchase type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public KalturaMonetizationType Type { get; set; }

        /// <summary>
        /// Mathermtical operator to calculate
        /// </summary>
        [DataMember(Name = "operator")]
        [JsonProperty(PropertyName = "operator")]
        [XmlElement(ElementName = "operator")]
        public KalturaMathemticalOperatorType Operator { get; set; }

        /// <summary>
        /// Comma saperated list of business module IDs
        /// </summary>
        [DataMember(Name = "businessModuleIdIn")]
        [JsonProperty(PropertyName = "businessModuleIdIn")]
        [XmlElement(ElementName = "businessModuleIdIn")]
        [SchemeProperty(IsNullable = true, MinLength = 1)]
        public string BusinessModuleIdIn { get; set; }

        /// <summary>
        /// Which currency code should be taken into consideration
        /// </summary>
        [DataMember(Name = "currencyCode")]
        [JsonProperty(PropertyName = "currencyCode")]
        [XmlElement(ElementName = "currencyCode")]
        [SchemeProperty(MinLength = 1, MaxLength = 4)]
        public string CurrencyCode { get; set; }
    }
}