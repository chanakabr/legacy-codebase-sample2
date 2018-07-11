using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Collection
    /// </summary>
    public partial class KalturaCollection : KalturaOTTObject
    {
        /// <summary>
        /// Collection identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// A list of channels associated with this collection 
        /// </summary>
        [DataMember(Name = "channels")]
        [JsonProperty("channels")]
        [XmlArray(ElementName = "channels", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBaseChannel> Channels { get; set; }

        /// <summary>
        /// The first date the collection is available for purchasing 
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long? StartDate { get; set; }

        /// <summary>
        /// The last date the collection is available for purchasing
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        public long? EndDate { get; set; }

        /// <summary>
        /// The internal discount module for the subscription
        /// </summary>
        [DataMember(Name = "discountModule")]
        [JsonProperty("discountModule")]
        [XmlElement(ElementName = "discountModule", IsNullable = true)]
        [OldStandardProperty("discount_module")]
        public KalturaDiscountModule DiscountModule { get; set; }

        /// <summary>
        /// Name of the subscription
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// description of the subscription
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description", IsNullable = true)]
        public KalturaMultilingualString Description { get; set; }


        /// <summary>
        /// Collection usage module
        /// </summary>
        [DataMember(Name = "usageModule")]
        [JsonProperty("usageModule")]
        [XmlElement(ElementName = "usageModule", IsNullable = true)]
        public KalturaUsageModule UsageModule { get; set; }

        /// <summary>
        /// List of Coupons group
        /// </summary>
        [DataMember(Name = "couponsGroups")]
        [JsonProperty("couponsGroups")]
        [XmlArray(ElementName = "couponsGroups", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaCouponsGroup> CouponGroups { get; set; }

        /// <summary>
        /// External ID
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        public string ExternalId{ get; set; }

        /// <summary>
        /// List of Collection product codes
        /// </summary>
        [DataMember(Name = "productCodes")]
        [JsonProperty("productCodes")]
        [XmlArray(ElementName = "productCodes", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaProductCode> ProductCodes { get; set; }

        /// <summary>
        /// The ID of the price details associated with this collection
        /// </summary>
        [DataMember(Name = "priceDetailsId")]
        [JsonProperty("priceDetailsId")]
        [XmlElement(ElementName = "priceDetailsId")]
        [SchemeProperty(MinInteger = 1)]
        public long? PriceDetailsId { get; set; }
    }

    /// <summary>
    /// Collections list
    /// </summary>
    [DataContract(Name = "Collections", Namespace = "")]
    [XmlRoot("Collections")]
    public partial class KalturaCollectionListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of collections
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaCollection> Collections { get; set; }
    }

    /// <summary>
    /// Collection Filter
    /// </summary>
    public partial class KalturaCollectionFilter : KalturaFilter<KalturaCollectionOrderBy>
    {
        /// <summary>
        /// Comma separated collection IDs
        /// </summary>
        [DataMember(Name = "collectionIdIn")]
        [JsonProperty("collectionIdIn")]
        [XmlElement(ElementName = "collectionIdIn")]
        [SchemeProperty(MinLength=1)]
        public string CollectionIdIn { get; set; }

        /// <summary>
        /// Media-file ID to get the subscriptions by
        /// </summary>
        [DataMember(Name = "mediaFileIdEqual")]
        [JsonProperty("mediaFileIdEqual")]
        [XmlElement(ElementName = "mediaFileIdEqual", IsNullable = true)]
        public int? MediaFileIdEqual { get; set; }

        public override KalturaCollectionOrderBy GetDefaultOrderByValue()
        {
            return KalturaCollectionOrderBy.NONE;
        }

        internal void Validate()
        {
            if (!MediaFileIdEqual.HasValue && string.IsNullOrEmpty(CollectionIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaCollectionFilter.collectionIdIn", "KalturaCollectionFilter.mediaFileIdEqual");
            }
            if (MediaFileIdEqual.HasValue && !string.IsNullOrEmpty(CollectionIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaCollectionFilter.collectionIdIn", "KalturaCollectionFilter.mediaFileIdEqual");
            }
        }

        internal string[] getCollectionIdIn()
        {
            if (string.IsNullOrEmpty(CollectionIdIn))
                return null;

            return CollectionIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public enum KalturaCollectionOrderBy
    {
        NONE = 0
    }
}