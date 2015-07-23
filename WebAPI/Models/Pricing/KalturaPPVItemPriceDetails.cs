using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// PPV item price details
    /// </summary>
    public class KalturaPPVItemPriceDetails : KalturaOTTObject
    {
        /// <summary>
        /// The associated PPV module identifier  
        /// </summary>
        [DataMember(Name = "ppv_module_id")]
        [JsonProperty("ppv_module_id")]
        public string PPVModuleId { get; set; }

        /// <summary>
        /// Denotes whether this object is available only as part of a subscription or can be sold separately
        /// </summary>
        [DataMember(Name = "is_subscription_only")]
        [JsonProperty("is_subscription_only")]
        public bool IsSubscriptionOnly { get; set; }

        /// <summary>
        /// The calculated price of the item after discounts (as part of a purchased subscription by the user or by using a coupon) 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        public KalturaPrice Price { get; set; }

        /// <summary>
        /// The full price of the item (with no discounts)
        /// </summary>
        [DataMember(Name = "full_price")]
        [JsonProperty("full_price")]
        public KalturaPrice FullPrice { get; set; }

        /// <summary>
        /// Subscription purchase status
        /// </summary>
        [DataMember(Name = "purchase_status")]
        [JsonProperty("purchase_status")]
        public KalturaPurchaseStatus PurchaseStatus { get; set; }

        /// <summary>
        /// The identifier of the relevant subscription
        /// </summary>
        [DataMember(Name = "subscription_id")]
        [JsonProperty("subscription_id")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The identifier of the relevant collection
        /// </summary>
        [DataMember(Name = "collection_id")]
        [JsonProperty("collection_id")]
        public string CollectionId { get; set; }

        /// <summary>
        /// The identifier of the relevant pre paid
        /// </summary>
        [DataMember(Name = "pre_paid_id")]
        [JsonProperty("pre_paid_id")]
        public string PrePaidId { get; set; }

        /// <summary>
        /// A list of the descriptions of the PPV module on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "ppv_descriptions")]
        [JsonProperty("ppv_descriptions")]
        public List<TranslationContainer> PPVDescriptions { get; set; }

        /// <summary>
        /// If the item already purchased - the identifier of the user (in the household) who purchased this item 
        /// </summary>
        [DataMember(Name = "purchase_user_id")]
        [JsonProperty("purchase_user_id")]
        public string PurchaseUserId { get; set; }

        /// <summary>
        /// If the item already purchased - the identifier of the purchased file
        /// </summary>
        [DataMember(Name = "purchased_media_file_id")]
        [JsonProperty("purchased_media_file_id")]
        public int PurchasedMediaFileId { get; set; }

        /// <summary>
        /// Related media files identifiers (different types)
        /// </summary>
        [DataMember(Name = "related_media_file_ids")]
        [JsonProperty("related_media_file_ids")]
        public List<int> RelatedMediaFileIds { get; set; }

        /// <summary>
        /// If the item already purchased - since when the user can start watching the item
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// If the item already purchased - until when the user can watch the item
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// If the item already purchased and played - the name of the device on which it was first played 
        /// </summary>
        [DataMember(Name = "first_device_name")]
        [JsonProperty("first_device_name")]
        public string FirstDeviceName { get; set; }

        /// <summary>
        /// If waiver period is enabled - donates whether the user is still in the cancelation window
        /// </summary>
        [DataMember(Name = "is_in_cancelation_period")]
        [JsonProperty("is_in_cancelation_period")]
        public bool IsInCancelationPeriod { get; set; }
    }
}