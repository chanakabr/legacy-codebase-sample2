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
    /// Coupon generation options
    /// </summary>
    public class KalturaCouponGenerationOptions : KalturaOTTObject
    {
    }
    public class KalturaPublicCouponGenerationOptions: KalturaCouponGenerationOptions
    {
        /// <summary>
        /// Coupon code (name)
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code", IsNullable = false)]
        public string Code { get; set; }
    }

    public class KalturaRandomCouponGenerationOptions : KalturaCouponGenerationOptions
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
        public bool UseLetters { get; set; }

        /// <summary>
        /// Indicates whether to use numbers in the generated codes (default is true)
        /// </summary>
        [DataMember(Name = "useNumbers")]
        [JsonProperty("useNumbers")]
        [XmlElement(ElementName = "useNumbers", IsNullable = true)]
        public bool UseNumbers{ get; set; }

        /// <summary>
        /// Indicates whether to use special characters in the generated codes(default is true)
        /// </summary>
        [DataMember(Name = "useSpecialCharacters")]
        [JsonProperty("useSpecialCharacters")]
        [XmlElement(ElementName = "useSpecialCharacters", IsNullable = true)]
        public bool UseSpecialCharacters { get; set; }
    }
}