using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaRandomCouponGenerationOptions : KalturaCouponGenerationOptions
    {
        /// <summary>
        /// Number of coupons to generate
        /// </summary>
        [DataMember(Name = "numberOfCoupons")]
        [JsonProperty("numberOfCoupons")]
        [XmlElement(ElementName = "numberOfCoupons", IsNullable = false)]
        public int NumberOfCoupons { get; set; }

        /// <summary>
        /// Indicates whether to use letters in the generated codes (default is true)
        /// </summary>
        [DataMember(Name = "useLetters")]
        [JsonProperty("useLetters")]
        [XmlElement(ElementName = "useLetters", IsNullable = true)]
        public bool? UseLetters { get; set; }

        /// <summary>
        /// Indicates whether to use numbers in the generated codes (default is true)
        /// </summary>
        [DataMember(Name = "useNumbers")]
        [JsonProperty("useNumbers")]
        [XmlElement(ElementName = "useNumbers", IsNullable = true)]
        public bool? UseNumbers { get; set; }

        /// <summary>
        /// Indicates whether to use special characters in the generated codes(default is true)
        /// </summary>
        [DataMember(Name = "useSpecialCharacters")]
        [JsonProperty("useSpecialCharacters")]
        [XmlElement(ElementName = "useSpecialCharacters", IsNullable = true)]
        public bool? UseSpecialCharacters { get; set; }
    }
}
