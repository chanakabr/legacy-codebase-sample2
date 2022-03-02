using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaProductPriceFilter : KalturaFilter<KalturaProductPriceOrderBy>
    {
        /// <summary>
        /// Comma separated subscriptions identifiers 
        /// </summary>
        [DataMember(Name = "subscriptionIdIn")]
        [JsonProperty("subscriptionIdIn")]
        [XmlElement(ElementName = "subscriptionIdIn")]
        public string SubscriptionIdIn { get; set; }

        /// <summary>
        /// Comma separated media files identifiers 
        /// </summary>
        [DataMember(Name = "fileIdIn")]
        [JsonProperty("fileIdIn")]
        [XmlElement(ElementName = "fileIdIn")]
        public string FileIdIn { get; set; }

        /// <summary>
        /// Comma separated collections identifiers 
        /// </summary>
        [DataMember(Name = "collectionIdIn")]
        [JsonProperty("collectionIdIn")]
        [XmlElement(ElementName = "collectionIdIn")]
        public string CollectionIdIn { get; set; }

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
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(FileIdIn, "KalturaProductPriceFilter.fileIdIn");
        }

        internal List<int> getSubscriptionIdIn()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(SubscriptionIdIn, "KalturaProductPriceFilter.subscriptionIdIn");
        }

        internal string[] getCollectionIdIn()
        {
            if (string.IsNullOrEmpty(CollectionIdIn))
                return null;

            return CollectionIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        internal void Validate()
        {
            if ((SubscriptionIdIn == null || SubscriptionIdIn.Count() == 0) && (FileIdIn == null || FileIdIn.Count() == 0) && (CollectionIdIn == null || CollectionIdIn.Count() == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaProductPriceFilter.subscriptionIdIn, KalturaProductPriceFilter.fileIdIn, KalturaProductPriceFilter.collectionIdIn");
            }
        }
    }
}