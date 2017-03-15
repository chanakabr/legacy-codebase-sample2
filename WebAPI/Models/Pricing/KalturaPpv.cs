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
    /// PPV details
    /// </summary>
    public class KalturaPpv : KalturaOTTObject
    {
        /// <summary>
        /// PPV identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// the name for the ppv
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The price of the ppv
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        public KalturaPriceDetails Price { get; set; }

        /// <summary>
        /// A list of file types identifiers that are supported in this ppv
        /// </summary>
        [DataMember(Name = "fileTypes")]
        [JsonProperty("fileTypes")]
        [XmlArray(ElementName = "fileTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> FileTypes { get; set; }

        /// <summary>
        /// The internal discount module for the ppv
        /// </summary>
        [DataMember(Name = "discountModule")]
        [JsonProperty("discountModule")]
        [XmlElement(ElementName = "discountModule", IsNullable = true)]
        public KalturaDiscountModule DiscountModule { get; set; }

        /// <summary>
        /// Coupons group for the ppv
        /// </summary>
        [DataMember(Name = "couponsGroup")]
        [JsonProperty("couponsGroup")]
        [XmlElement(ElementName = "couponsGroup", IsNullable = true)]
        public KalturaCouponsGroup CouponsGroup { get; set; }

        /// <summary>
        /// A list of the descriptions of the ppv on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Descriptions { get; set; } // TODO: change to object

        /// <summary>
        /// Product code for the ppv
        /// </summary>
        [DataMember(Name = "productCode")]
        [JsonProperty("productCode")]
        [XmlElement(ElementName = "productCode")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Indicates whether or not this ppv can be purchased standalone or only as part of a subscription
        /// </summary>
        [DataMember(Name = "isSubscriptionOnly")]
        [JsonProperty("isSubscriptionOnly")]
        [XmlElement(ElementName = "isSubscriptionOnly", IsNullable = true)]
        public bool? IsSubscriptionOnly { get; set; }

        /// <summary>
        /// Indicates whether or not this ppv can be consumed only on the first device
        /// </summary>
        [DataMember(Name = "firstDeviceLimitation")]
        [JsonProperty("firstDeviceLimitation")]
        [XmlElement(ElementName = "firstDeviceLimitation", IsNullable = true)]
        public bool? FirstDeviceLimitation { get; set; }

        /// <summary>
        /// PPV usage module
        /// </summary>
        [DataMember(Name = "usageModule")]
        [JsonProperty("usageModule")]
        [XmlElement(ElementName = "usageModule", IsNullable = true)]
        public KalturaUsageModule UsageModule { get; set; }
    }
}