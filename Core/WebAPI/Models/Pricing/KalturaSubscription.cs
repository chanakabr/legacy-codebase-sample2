using ApiObjects.Pricing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscription details
    /// </summary>
    public partial class KalturaSubscription : KalturaOTTObjectSupportNullable
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
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public List<KalturaBaseChannel> Channels { get; set; }

        /// <summary>
        /// Comma separated channels Ids associated with this subscription
        /// </summary>
        [DataMember(Name = "channelsIds")]
        [JsonProperty("channelsIds")]
        [XmlArray(ElementName = "channelsIds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string ChannelsIds { get; set; }

        /// <summary>
        /// The first date the subscription is available for purchasing 
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        [OldStandardProperty("start_date")]
        [SchemeProperty(IsNullable = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// The last date the subscription is available for purchasing
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        [OldStandardProperty("end_date")]
        [SchemeProperty(IsNullable = true)]
        public long? EndDate { get; set; }

        /// <summary>
        /// A list of file types identifiers that are supported in this subscription
        /// </summary>
        [DataMember(Name = "fileTypes")]
        [JsonProperty("fileTypes")]
        [XmlArray(ElementName = "fileTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("file_types")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public List<KalturaIntegerValue> FileTypes { get; set; }

        /// <summary>
        /// Comma separated file types identifiers that are supported in this subscription
        /// </summary>
        [DataMember(Name = "fileTypesIds")]
        [JsonProperty("fileTypesIds")]
        [XmlArray(ElementName = "fileTypesIds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string FileTypesIds { get; set; }

        /// <summary>
        /// Denotes whether or not this subscription can be renewed
        /// </summary>
        [DataMember(Name = "isRenewable")]
        [JsonProperty("isRenewable")]
        [XmlElement(ElementName = "isRenewable")]
        [OldStandardProperty("is_renewable")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public bool? IsRenewable { get; set; }

        /// <summary>
        /// Defines the number of times this subscription will be renewed
        /// </summary>
        [DataMember(Name = "renewalsNumber")]
        [JsonProperty("renewalsNumber")]
        [XmlElement(ElementName = "renewalsNumber")]
        [OldStandardProperty("renewals_number")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public int? RenewalsNumber { get; set; }

        /// <summary>
        /// Indicates whether the subscription will renew forever
        /// </summary>
        [DataMember(Name = "isInfiniteRenewal")]
        [JsonProperty("isInfiniteRenewal")]
        [XmlElement(ElementName = "isInfiniteRenewal")]
        [OldStandardProperty("is_infinite_renewal")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public bool? IsInfiniteRenewal { get; set; }

        /// <summary>
        /// The price of the subscription
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public KalturaPriceDetails Price { get; set; }

        /// <summary>
        /// The internal discount module for the subscription
        /// </summary>
        [DataMember(Name = "discountModule")]
        [JsonProperty("discountModule")]
        [XmlElement(ElementName = "discountModule", IsNullable = true)]
        [OldStandardProperty("discount_module")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public KalturaDiscountModule DiscountModule { get; set; }

        /// <summary>
        /// The internal discount module identifier for the subscription
        /// </summary>
        [DataMember(Name = "internalDiscountModuleId")]
        [JsonProperty("internalDiscountModuleId")]
        [XmlElement(ElementName = "internalDiscountModuleId", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        public long? InternalDiscountModuleId { get; set; }

        /// <summary>
        /// Coupons group for the subscription
        /// </summary>
        [DataMember(Name = "couponsGroup")]
        [JsonProperty("couponsGroup")]
        [XmlElement(ElementName = "couponsGroup", IsNullable = true)]
        [OldStandardProperty("coupons_group")]
        [Deprecated("4.3.0.0")]
        public KalturaCouponsGroup CouponsGroup { get; set; }

        /// <summary>
        /// Name of the subscription
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// A list of the name of the subscription on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "names")]
        [JsonProperty("names")]
        [XmlArray(ElementName = "names", IsNullable = true)]
        [XmlArrayItem("item")]
        [Deprecated("3.6.287.27312")]
        public List<KalturaTranslationToken> Names { get; set; } // TODO: change to object

        /// <summary>
        /// description of the subscription
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaMultilingualString Description { get; set; }

        /// <summary>
        /// A list of the descriptions of the subscriptions on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        [Deprecated("3.6.287.27312")]
        public List<KalturaTranslationToken> Descriptions { get; set; } // TODO: change to object

        /// <summary>
        /// Identifier of the media associated with the subscription
        /// </summary>
        [DataMember(Name = "mediaId")]
        [JsonProperty("mediaId")]
        [XmlElement(ElementName = "mediaId")]
        [OldStandardProperty("media_id")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public int? MediaId { get; set; }

        /// <summary>
        /// Subscription order (when returned in methods that retrieve subscriptions)
        /// </summary>
        [DataMember(Name = "prorityInOrder")]
        [JsonProperty("prorityInOrder")]
        [XmlElement(ElementName = "prorityInOrder")]
        [OldStandardProperty("prority_in_order")]
        [SchemeProperty(IsNullable = true)]
        public long? ProrityInOrder { get; set; }

        /// <summary>
        /// Product code for the subscription
        /// </summary>
        [DataMember(Name = "productCode")]
        [JsonProperty("productCode")]
        [XmlElement(ElementName = "productCode")]
        [OldStandardProperty("product_code")]
        [Deprecated("4.3.0.0")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Subscription price plans
        /// </summary>
        [DataMember(Name = "pricePlans")]
        [JsonProperty("pricePlans")]
        [XmlArray(ElementName = "pricePlans", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("price_plans")]
        [Deprecated("4.5.0.0")]
        public List<KalturaPricePlan> PricePlans { get; set; }

        /// <summary>
        /// Comma separated subscription price plan IDs
        /// </summary>
        [DataMember(Name = "pricePlanIds")]
        [JsonProperty("pricePlanIds")]
        [XmlElement(ElementName = "pricePlanIds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string PricePlanIds { get; set; }

        /// <summary>
        /// Subscription preview module
        /// </summary>
        [DataMember(Name = "previewModule")]
        [JsonProperty("previewModule")]
        [XmlElement(ElementName = "previewModule", IsNullable = true)]
        [OldStandardProperty("preview_module")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public KalturaPreviewModule PreviewModule { get; set; }

        /// <summary>
        /// Subscription preview module identifier
        /// </summary>
        [DataMember(Name = "previewModuleId")]
        [JsonProperty("previewModuleId")]
        [XmlElement(ElementName = "previewModuleId", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        public long PreviewModuleId { get; set; }

        /// <summary>
        /// The household limitation module identifier associated with this subscription
        /// </summary>
        [DataMember(Name = "householdLimitationsId")]
        [JsonProperty("householdLimitationsId")]
        [XmlElement(ElementName = "householdLimitationsId")]
        [OldStandardProperty("household_limitations_id")]
        [SchemeProperty(IsNullable = true)]
        public int? HouseholdLimitationsId { get; set; }

        /// <summary>
        /// The subscription grace period in minutes
        /// </summary>
        [DataMember(Name = "gracePeriodMinutes")]
        [JsonProperty("gracePeriodMinutes")]
        [XmlElement(ElementName = "gracePeriodMinutes")]
        [OldStandardProperty("grace_period_minutes")]
        [SchemeProperty(IsNullable = true)]
        public int? GracePeriodMinutes { get; set; }

        /// <summary>
        /// List of premium services included in the subscription
        /// </summary>
        [DataMember(Name = "premiumServices")]
        [JsonProperty("premiumServices")]
        [XmlArray(ElementName = "premiumServices", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("premium_services")]
        [SchemeProperty(IsNullable = true)]
        public List<KalturaPremiumService> PremiumServices { get; set; }

        #region Usage Module

        /// <summary>
        /// The maximum number of times an item in this usage module can be viewed
        /// </summary>
        [DataMember(Name = "maxViewsNumber")]
        [JsonProperty("maxViewsNumber")]
        [XmlElement(ElementName = "maxViewsNumber")]
        [OldStandardProperty("max_views_number")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public int? MaxViewsNumber { get; set; }

        /// <summary>
        /// The amount time an item is available for viewing since a user started watching the item
        /// </summary>
        [DataMember(Name = "viewLifeCycle")]
        [JsonProperty("viewLifeCycle")]
        [XmlElement(ElementName = "viewLifeCycle")]
        [OldStandardProperty("view_life_cycle")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public int? ViewLifeCycle { get; set; }

        /// <summary>
        /// Time period during which the end user can waive his rights to cancel a purchase. When the time period is passed, the purchase can no longer be cancelled
        /// </summary>
        [DataMember(Name = "waiverPeriod")]
        [JsonProperty("waiverPeriod")]
        [XmlElement(ElementName = "waiverPeriod")]
        [OldStandardProperty("waiver_period")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public int? WaiverPeriod { get; set; }

        /// <summary>
        /// Indicates whether or not the end user has the right to waive his rights to cancel a purchase
        /// </summary>
        [DataMember(Name = "isWaiverEnabled")]
        [JsonProperty("isWaiverEnabled")]
        [XmlElement(ElementName = "isWaiverEnabled")]
        [OldStandardProperty("is_waiver_enabled")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public bool? IsWaiverEnabled { get; set; }

        /// <summary>
        /// List of permitted user types for the subscription
        /// </summary>
        [DataMember(Name = "userTypes")]
        [JsonProperty("userTypes")]
        [XmlArray(ElementName = "userTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("user_types")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public List<KalturaOTTUserType> UserTypes { get; set; }

        #endregion

        /// <summary>
        /// List of Coupons group
        /// </summary>
        [DataMember(Name = "couponsGroups")]
        [JsonProperty("couponsGroups")]
        [XmlArray(ElementName = "couponsGroups", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public List<KalturaCouponsGroup> CouponGroups { get; set; }

        /// <summary>
        /// List of subscription Coupons group
        /// </summary>
        [DataMember(Name = "subscriptionCouponGroup")]
        [JsonProperty("subscriptionCouponGroup")]
        [XmlArray(ElementName = "subscriptionCouponGroup", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true)]
        public List<KalturaSubscriptionCouponGroup> SubscriptionCouponGroup { get; set; }

        /// <summary>
        /// List of Subscription product codes
        /// </summary>
        [DataMember(Name = "productCodes")]
        [JsonProperty("productCodes")]
        [XmlArray(ElementName = "productCodes", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true)]
        public List<KalturaProductCode> ProductCodes { get; set; }

        /// <summary>
        ///Dependency Type
        /// </summary>
        [DataMember(Name = "dependencyType")]
        [JsonProperty("dependencyType")]
        [XmlElement(ElementName = "dependencyType")]
        public KalturaSubscriptionDependencyType DependencyType { get; set; }

        /// <summary>
        /// External ID
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Is cancellation blocked for the subscription
        /// </summary>
        [DataMember(Name = "isCancellationBlocked")]
        [JsonProperty("isCancellationBlocked")]
        [XmlElement(ElementName = "isCancellationBlocked")]
        public bool IsCancellationBlocked { get; set; }

        /// <summary>
        /// The Pre-Sale date the subscription is available for purchasing         
        /// </summary>
        [DataMember(Name = "preSaleDate")]
        [JsonProperty("preSaleDate")]
        [XmlElement(ElementName = "preSaleDate")]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        public long? PreSaleDate { get; set; }

        /// <summary>
        /// Ads policy 
        /// </summary>
        [DataMember(Name = "adsPolicy")]
        [JsonProperty("adsPolicy")]
        [XmlElement(ElementName = "adsPolicy")]
        public KalturaAdsPolicy? AdsPolicy { get; set; }

        /// <summary>
        /// The parameters to pass to the ads server 
        /// </summary>
        [DataMember(Name = "adsParam")]
        [JsonProperty("adsParam")]
        [XmlElement(ElementName = "adsParam")]
        public string AdsParams { get; set; }

        /// <summary>
        /// Is active subscription
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Specifies when was the Subscription created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Subscription last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        public void ValidateForAdd()
        {
            if (!string.IsNullOrEmpty(ChannelsIds))
            {
                _ = GetItemsIn<List<long>, long>(ChannelsIds, "channelsIds", true);
            }

            if (CouponGroups?.Count > 0)
            {
                CouponGroups.ForEach(x => x.Validate());
            }

            if (this.Name == null || this.Name.Values == null || this.Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
            this.Name.Validate("multilingualName");

            if (this.Description != null && this.Description.Values != null && this.Description.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
            }

            if (this.Description != null)
            {
                this.Description.Validate("multilingualDescription");
            }

            if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDate", "endDate");
            }

            if (!string.IsNullOrEmpty(FileTypesIds))
            {
                _ = GetItemsIn<List<long>, long>(FileTypesIds, "fileTypesIds", true);
            }

            if (ProductCodes?.Count > 1)
            {
                List<string> res = new List<string>();

                foreach (var item in ProductCodes)
                {
                    if (res.Contains(item.InappProvider))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "KalturaProductCode.InappProvider");
                    }

                    if (!Enum.TryParse(item.InappProvider, out VerificationPaymentGateway tmp))
                    {
                        throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "KalturaProductCode.InappProvider", item.InappProvider);
                    }

                    res.Add(item.InappProvider);
                }
            }
        }

        internal void ValidateForUpdate()
        {
        }
    }
}