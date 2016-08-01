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
    /// Subscription details
    /// </summary>
    [OldStandard("startDate", "start_date")]
    [OldStandard("endDate", "end_date")]
    [OldStandard("fileTypes", "file_types")]
    [OldStandard("isRenewable", "is_renewable")]
    [OldStandard("renewalsNumber", "renewals_number")]
    [OldStandard("isInfiniteRenewal", "is_infinite_renewal")]
    [OldStandard("discountModule", "discount_module")]
    [OldStandard("couponsGroup", "coupons_group")]
    [OldStandard("mediaId", "media_id")]
    [OldStandard("prorityInOrder", "prority_in_order")]
    [OldStandard("productCode", "product_code")]
    [OldStandard("pricePlans", "price_plans")]
    [OldStandard("previewModule", "preview_module")]
    [OldStandard("householdLimitationsId", "household_limitations_id")]
    [OldStandard("gracePeriodMinutes", "grace_period_minutes")]
    [OldStandard("premiumServices", "premium_services")]
    [OldStandard("maxViewsNumber", "max_views_number")]
    [OldStandard("viewLifeCycle", "view_life_cycle")]
    [OldStandard("waiverPeriod", "waiver_period")]
    [OldStandard("isWaiverEnabled", "is_waiver_enabled")]
    [OldStandard("userTypes", "user_types")]
    public class KalturaSubscription : KalturaOTTObject
    {
        /// <summary>
        /// Subscription identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// A list of channels associated with this subscription 
        /// </summary>
        [DataMember(Name = "channels")]
        [JsonProperty("channels")]
        [XmlArray(ElementName = "channels", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBaseChannel> Channels { get; set; }

        /// <summary>
        /// The first date the subscription is available for purchasing 
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long? StartDate { get; set; }

        /// <summary>
        /// The last date the subscription is available for purchasing
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        public long? EndDate { get; set; }

        /// <summary>
        /// A list of file types identifiers that are supported in this subscription
        /// </summary>
        [DataMember(Name = "fileTypes")]
        [JsonProperty("fileTypes")]
        [XmlArray(ElementName = "fileTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> FileTypes { get; set; }

        /// <summary>
        /// Denotes whether or not this subscription can be renewed
        /// </summary>
        [DataMember(Name = "isRenewable")]
        [JsonProperty("isRenewable")]
        [XmlElement(ElementName = "isRenewable")]
        public bool? IsRenewable { get; set; }

        /// <summary>
        /// Defines the number of times this subscription will be renewed
        /// </summary>
        [DataMember(Name = "renewalsNumber")]
        [JsonProperty("renewalsNumber")]
        [XmlElement(ElementName = "renewalsNumber")]
        public int? RenewalsNumber { get; set; }

        /// <summary>
        /// Indicates whether the subscription will renew forever
        /// </summary>
        [DataMember(Name = "isInfiniteRenewal")]
        [JsonProperty("isInfiniteRenewal")]
        [XmlElement(ElementName = "isInfiniteRenewal")]
        public bool? IsInfiniteRenewal { get; set; }

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
        public KalturaDiscountModule DiscountModule { get; set; }

        /// <summary>
        /// Coupons group for the subscription
        /// </summary>
        [DataMember(Name = "couponsGroup")]
        [JsonProperty("couponsGroup")]
        [XmlElement(ElementName = "couponsGroup", IsNullable = true)]
        public KalturaCouponsGroup CouponsGroup { get; set; }

        /// <summary>
        /// A list of the name of the subscription on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "names")]
        [JsonProperty("names")]
        [XmlArray(ElementName = "names", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Names { get; set; } // TODO: change to object

        /// <summary>
        /// A list of the descriptions of the subscriptions on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Descriptions { get; set; } // TODO: change to object

        /// <summary>
        /// Identifier of the media associated with the subscription
        /// </summary>
        [DataMember(Name = "mediaId")]
        [JsonProperty("mediaId")]
        [XmlElement(ElementName = "mediaId")]
        public int? MediaId { get; set; }

        /// <summary>
        /// Subscription order (when returned in methods that retrieve subscriptions)
        /// </summary>
        [DataMember(Name = "prorityInOrder")]
        [JsonProperty("prorityInOrder")]
        [XmlElement(ElementName = "prorityInOrder")]
        public long? ProrityInOrder { get; set; }

        /// <summary>
        /// Product code for the subscription
        /// </summary>
        [DataMember(Name = "productCode")]
        [JsonProperty("productCode")]
        [XmlElement(ElementName = "productCode")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Subscription price plans
        /// </summary>
        [DataMember(Name = "pricePlans")]
        [JsonProperty("pricePlans")]
        [XmlArray(ElementName = "pricePlans", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPricePlan> PricePlans { get; set; }

        /// <summary>
        /// Subscription preview module
        /// </summary>
        [DataMember(Name = "previewModule")]
        [JsonProperty("previewModule")]
        [XmlElement(ElementName = "previewModule", IsNullable = true)]
        public KalturaPreviewModule PreviewModule { get; set; }

        /// <summary>
        /// The household limitation module identifier associated with this subscription
        /// </summary>
        [DataMember(Name = "householdLimitationsId")]
        [JsonProperty("householdLimitationsId")]
        [XmlElement(ElementName = "householdLimitationsId")]
        public int? HouseholdLimitationsId { get; set; }

        /// <summary>
        /// The subscription grace period in minutes
        /// </summary>
        [DataMember(Name = "gracePeriodMinutes")]
        [JsonProperty("gracePeriodMinutes")]
        [XmlElement(ElementName = "gracePeriodMinutes")]
        public int? GracePeriodMinutes { get; set; }

        /// <summary>
        /// List of premium services included in the subscription
        /// </summary>
        [DataMember(Name = "premiumServices")]
        [JsonProperty("premiumServices")]
        [XmlArray(ElementName = "premiumServices", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPremiumService> PremiumServices { get; set; }

        #region Usage Module

        /// <summary>
        /// The maximum number of times an item in this usage module can be viewed
        /// </summary>
        [DataMember(Name = "maxViewsNumber")]
        [JsonProperty("maxViewsNumber")]
        [XmlElement(ElementName = "maxViewsNumber")]
        public int? MaxViewsNumber { get; set; }

        /// <summary>
        /// The amount time an item is available for viewing since a user started watching the item
        /// </summary>
        [DataMember(Name = "viewLifeCycle")]
        [JsonProperty("viewLifeCycle")]
        [XmlElement(ElementName = "viewLifeCycle")]
        public int? ViewLifeCycle { get; set; }

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
        /// List of permitted user types for the subscription
        /// </summary>
        [DataMember(Name = "userTypes")]
        [JsonProperty("userTypes")]
        [XmlArray(ElementName = "userTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaOTTUserType> UserTypes { get; set; }

        #endregion
    }
}