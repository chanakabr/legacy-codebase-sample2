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
        [DataMember(Name = "discountModules")]
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
        /// Time period during which the end user can waive his rights to cancel a purchase. When the time period is passed, the purchase can no longer be cancelled
        /// </summary>
        [DataMember(Name = "waiverPeriod")]
        [JsonProperty("waiverPeriod")]
        [XmlElement(ElementName = "waiverPeriod")]
        public int? WaiverPeriod { get; set; }

        /// <summary>
        /// Indicates whether or not the end user has the right to waive his rights to cancel a purchase
        /// </summary>
        [DataMember(Name = "isWaiverEnabled")]
        [JsonProperty("isWaiverEnabled")]
        [XmlElement(ElementName = "isWaiverEnabled")]
        public bool? IsWaiverEnabled { get; set; }

        /// <summary>
        /// Indicates whether or not this ppv can be purchased standalone or only as part of a subscription
        /// </summary>
        [DataMember(Name = "isSubscriptionOnly")]
        [JsonProperty("isSubscriptionOnly")]
        [XmlElement(ElementName = "isSubscriptionOnly")]
        public bool? IsSubscriptionOnly { get; set; }

        /// <summary>
        /// virtual name for the ppv
        /// </summary>
        [DataMember(Name = "virtualName")]
        [JsonProperty("virtualName")]
        [XmlElement(ElementName = "virtualName")]
        public string VirtualName { get; set; }
    }
}