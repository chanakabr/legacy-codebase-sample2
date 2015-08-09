using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscription details
    /// </summary>
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
        [XmlArray(ElementName = "channels")]
        [XmlArrayItem("item")] 
        public List<KalturaSlimChannel> Channels { get; set; } 

        /// <summary>
        /// The first date the subscription is available for purchasing 
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty("start_date")]
        [XmlElement(ElementName = "start_date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The last date the subscription is available for purchasing
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        [XmlElement(ElementName = "end_date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// A list of file types identifiers that are supported in this subscription
        /// </summary>
        [DataMember(Name = "file_types")]
        [JsonProperty("file_types")]
        [XmlArray(ElementName = "file_types")]
        [XmlArrayItem("item")] 
        public List<int> FileTypes { get; set; }
        
        /// <summary>
        /// Denotes whether or not this subscription can be renewed
        /// </summary>
        [DataMember(Name = "is_renewable")]
        [JsonProperty("is_renewable")]
        [XmlElement(ElementName = "is_renewable")]
        public bool IsRenewable { get; set; }

        /// <summary>
        /// Defines the number of times this subscription will be renewed
        /// </summary>
        [DataMember(Name = "renewals_number")]
        [JsonProperty("renewals_number")]
        [XmlElement(ElementName = "renewals_number")]
        public int RenewalsNumber { get; set; }

        /// <summary>
        /// Indicates whether the subscription will renew forever
        /// </summary>
        [DataMember(Name = "is_infinite_renewal")]
        [JsonProperty("is_infinite_renewal")]
        [XmlElement(ElementName = "is_infinite_renewal")]
        public bool IsInfiniteRenewal { get; set; }

        /// <summary>
        /// The price of the subscription
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price")]
        public KalturaPriceDetails Price { get; set; }

        /// <summary>
        /// The discount module for the subscription
        /// </summary>
        [DataMember(Name = "discount_module")]
        [JsonProperty("discount_module")]
        [XmlElement(ElementName = "discount_module")]
        public KalturaDiscountModule DiscountModule { get; set; }

        /// <summary>
        /// Coupons group for the subscription
        /// </summary>
        [DataMember(Name = "coupons_group")]
        [JsonProperty("coupons_group")]
        [XmlElement(ElementName = "coupons_group")]
        public KalturaCouponsGroup CouponsGroup { get; set; }

        /// <summary>
        /// A list of the name of the subscription on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "names")]
        [JsonProperty("names")]
        [XmlArray(ElementName = "names")]
        [XmlArrayItem("item")] 
        public List<KalturaTranslationContainer> Names { get; set; } // TODO: change to object

        /// <summary>
        /// A list of the descriptions of the subscriptions on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions")]
        [XmlArrayItem("item")] 
        public List<KalturaTranslationContainer> Descriptions { get; set; } // TODO: change to object

        /// <summary>
        /// Identifier of the media associated with the subscription
        /// </summary>
        [DataMember(Name = "media_id")]
        [JsonProperty("media_id")]
        [XmlElement(ElementName = "media_id")]
        public int MediaId { get; set; }

        /// <summary>
        /// Subscription order (when returned in methods that retrieve subscriptions)
        /// </summary>
        [DataMember(Name = "prority_in_order")]
        [JsonProperty("prority_in_order")]
        [XmlElement(ElementName = "prority_in_order")]
        public long ProrityInOrder { get; set; }

        /// <summary>
        /// Product code for the subscription
        /// </summary>
        [DataMember(Name = "product_code")]
        [JsonProperty("product_code")]
        [XmlElement(ElementName = "product_code")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Subscription price plans
        /// </summary>
        [DataMember(Name = "price_plans")]
        [JsonProperty("price_plans")]
        [XmlArray(ElementName = "price_plans")]
        [XmlArrayItem("item")] 
        public List<KalturaPricePlan> PricePlans { get; set; }

        /// <summary>
        /// Subscription preview module
        /// </summary>
        [DataMember(Name = "preview_module")]
        [JsonProperty("preview_module")]
        [XmlElement(ElementName = "preview_module")]
        public KalturaPreviewModule PreviewModule { get; set; }

        /// <summary>
        /// The domain limitation module identifier associated with this subscription
        /// </summary>
        [DataMember(Name = "dlm_id")]
        [JsonProperty("dlm_id")]
        [XmlElement(ElementName = "dlm_id")]
        public int DlmId { get; set; }

        /// <summary>
        /// List of premium services included in the subscription
        /// </summary>
        [DataMember(Name = "premium_services")]
        [JsonProperty("premium_services")]
        [XmlArray(ElementName = "premium_services")]
        [XmlArrayItem("item")] 
        public List<KalturaPremiumService> PremiumServices { get; set; }

        #region Usage Module

        /// <summary>
        /// The maximum number of times an item in this usage module can be viewed
        /// </summary>
        [DataMember(Name = "max_views_number")]
        [JsonProperty("max_views_number")]
        [XmlElement(ElementName = "max_views_number")]
        public int MaxViewsNumber { get; set; }

        /// <summary>
        /// The amount time an item is available for viewing since a user started watching the item
        /// </summary>
        [DataMember(Name = "view_life_cycle")]
        [JsonProperty("view_life_cycle")]
        [XmlElement(ElementName = "view_life_cycle")]
        public int ViewLifeCycle { get; set; }

        /// <summary>
        /// Time period during which the end user can waive his rights to cancel a purchase. When the time period is passed, the purchase can no longer be cancelled
        /// </summary>
        [DataMember(Name = "waiver_period")]
        [JsonProperty("waiver_period")]
        [XmlElement(ElementName = "waiver_period")]
        public int WaiverPeriod { get; set; }

        /// <summary>
        /// Indicates whether or not the end user has the right to waive his rights to cancel a purchase
        /// </summary>
        [DataMember(Name = "is_waiver_enabled")]
        [JsonProperty("is_waiver_enabled")]
        [XmlElement(ElementName = "is_waiver_enabled")]
        public bool IsWaiverEnabled { get; set; }

        /// <summary>
        /// List of permitted user types for the subscription
        /// </summary>
        [DataMember(Name = "user_types")]
        [JsonProperty("user_types")]
        [XmlArray(ElementName = "user_types")]
        [XmlArrayItem("item")] 
        public List<KalturaOTTUserType> UserTypes { get; set; }

        #endregion
    }
}