using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaProductPriceFilter : KalturaFilter<KalturaProductPriceOrderBy>
    {
        /// <summary>
        /// Comma separated subscriptions identifiers 
        /// </summary>
        [DataMember(Name = "subscriptionIdIn")]
        [JsonProperty("subscriptionIdIn")]
        [XmlArray(ElementName = "subscriptionIdIn", IsNullable = true)]
        [XmlArrayItem("item")]
        public string SubscriptionIdIn { get; set; }

        /// <summary>
        /// Comma separated media files identifiers 
        /// </summary>
        [DataMember(Name = "fileIdIn")]
        [JsonProperty("fileIdIn")]
        [XmlArray(ElementName = "fileIdIn", IsNullable = true)]
        [XmlArrayItem("item")]
        public string FileIdIn { get; set; }

        /// <summary>
        /// A flag that indicates if only the lowest price of an item should return
        /// </summary>
        [DataMember(Name = "isLowest")]
        [JsonProperty("isLowest")]
        [XmlElement(ElementName = "isLowest")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
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

        internal List<int> getFileIdIn()
        {
            if (string.IsNullOrEmpty(FileIdIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = FileIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.FileIdIn contains invalid id {0}", value));
                }
            }

            return values;
        }

        internal List<int> getSubscriptionIdIn()
        {
            if (string.IsNullOrEmpty(SubscriptionIdIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = SubscriptionIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.SubscriptionIdIn contains invalid id {0}", value));
                }
            }

            return values;
        }
    }
}