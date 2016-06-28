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
    public class KalturaPriceFilter : KalturaFilter<KalturaPriceOrderBy>
    {
        /// <summary>
        /// Subscriptions Identifiers 
        /// </summary>
        [DataMember(Name = "subscriptionsIdIn")]
        [JsonProperty("subscriptionsIdIn")]
        [XmlArray(ElementName = "subscriptionsIdIn", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> SubscriptionsIdIn { get; set; }

        /// <summary>
        /// Media files Identifiers 
        /// </summary>
        [DataMember(Name = "filesIdIn")]
        [JsonProperty("filesIdIn")]
        [XmlArray(ElementName = "filesIdIn", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> FilesIdIn { get; set; }

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

        public override KalturaPriceOrderBy GetDefaultOrderByValue()
        {
            return KalturaPriceOrderBy.CREATE_DATE_ASC;
        }
    }
}