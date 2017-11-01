using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
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
    public class KalturaCollection : KalturaOTTObject
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
        /// The price of the subscription
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        public KalturaPriceDetails Price { get; set; }

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
        /// Comma separated subscription price plan IDs
        /// </summary>
        [DataMember(Name = "pricePlanIds")]
        [JsonProperty("pricePlanIds")]
        [XmlElement(ElementName = "pricePlanIds", IsNullable = true)]
        public string PricePlanIds { get; set; }

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
    }

    /// <summary>
    /// Collections list
    /// </summary>
    [DataContract(Name = "Collections", Namespace = "")]
    [XmlRoot("Collections")]
    public class KalturaCollectionListResponse : KalturaListResponse
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
    public class KalturaCollectionFilter : KalturaFilter<KalturaCollectionOrderBy>
    {
        /// <summary>
        /// Comma separated collection IDs
        /// </summary>
        [DataMember(Name = "collectionIdIn")]
        [JsonProperty("collectionIdIn")]
        [XmlElement(ElementName = "collectionIdIn")]
        [SchemeProperty(MinLength=1)]
        public string CollectionIdIn { get; set; }

        public override KalturaCollectionOrderBy GetDefaultOrderByValue()
        {
            return KalturaCollectionOrderBy.NONE;
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