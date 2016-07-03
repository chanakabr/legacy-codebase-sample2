using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaProductPriceFilter : KalturaFilter<KalturaProductPriceOrderBy>
    {
        /// <summary>
        /// Subscriptions Identifiers 
        /// </summary>
        [DataMember(Name = "subscriptionIdIn")]
        [JsonProperty("subscriptionIdIn")]
        [XmlArray(ElementName = "subscriptionIdIn", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> SubscriptionIdIn { get; set; }

        /// <summary>
        /// Media files Identifiers 
        /// </summary>
        [DataMember(Name = "fileIdIn")]
        [JsonProperty("fileIdIn")]
        [XmlArray(ElementName = "fileIdIn", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> FileIdIn { get; set; }

        /// <summary>
        /// A flag that indicates if only the lowest price of an item should return
        /// </summary>
        [DataMember(Name = "isLowest")]
        [JsonProperty("isLowest")]
        [XmlElement(ElementName = "isLowest")]
        [ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        public bool? isLowest { get; set; }

        /// <summary>
        /// Discount coupon code
        /// </summary>
        [DataMember(Name = "couponCodeEqual")]
        [JsonProperty("couponCodeEqual")]
        [XmlElement(ElementName = "couponCodeEqual")]
        public string CouponCodeEqual { get; set; }

        internal bool getShouldGetOnlyLowest()
        {
            return isLowest.HasValue ? (bool)isLowest : false;
        }

        public override KalturaProductPriceOrderBy GetDefaultOrderByValue()
        {
            return KalturaProductPriceOrderBy.PRODUCT_ID_ASC;
        }
    }
}