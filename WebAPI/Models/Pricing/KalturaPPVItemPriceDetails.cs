using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
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
        [DataMember(Name = "ppvModuleId")]
        [JsonProperty("ppvModuleId")]
        [XmlElement(ElementName = "ppvModuleId")]
        [OldStandardProperty("ppv_module_id")]
        public string PPVModuleId { get; set; }

        /// <summary>
        /// Denotes whether this object is available only as part of a subscription or can be sold separately
        /// </summary>
        [DataMember(Name = "isSubscriptionOnly")]
        [JsonProperty("isSubscriptionOnly")]
        [XmlElement(ElementName = "isSubscriptionOnly")]
        [OldStandardProperty("is_subscription_only")]
        public bool? IsSubscriptionOnly { get; set; }

        /// <summary>
        /// The calculated price of the item after discounts (as part of a purchased subscription by the user or by using a coupon) 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        public KalturaPrice Price { get; set; }

        /// <summary>
        /// The full price of the item (with no discounts)
        /// </summary>
        [DataMember(Name = "fullPrice")]
        [JsonProperty("fullPrice")]
        [XmlElement(ElementName = "fullPrice", IsNullable = true)]
        [OldStandardProperty("full_price")]
        public KalturaPrice FullPrice { get; set; }

        /// <summary>
        /// Subscription purchase status
        /// </summary>
        [DataMember(Name = "purchaseStatus")]
        [JsonProperty("purchaseStatus")]
        [XmlElement(ElementName = "purchaseStatus")]
        [OldStandardProperty("purchase_status")]
        public KalturaPurchaseStatus PurchaseStatus { get; set; }

        /// <summary>
        /// The identifier of the relevant subscription
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        [JsonProperty("subscriptionId")]
        [XmlElement(ElementName = "subscriptionId")]
        [OldStandardProperty("subscription_id")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The identifier of the relevant collection
        /// </summary>
        [DataMember(Name = "collectionId")]
        [JsonProperty("collectionId")]
        [XmlElement(ElementName = "collectionId")]
        [OldStandardProperty("collection_id")]
        public string CollectionId { get; set; }

        /// <summary>
        /// The identifier of the relevant pre paid
        /// </summary>
        [DataMember(Name = "prePaidId")]
        [JsonProperty("prePaidId")]
        [XmlElement(ElementName = "prePaidId")]
        [OldStandardProperty("pre_paid_id")]
        public string PrePaidId { get; set; }

        /// <summary>
        /// A list of the descriptions of the PPV module on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "ppvDescriptions")]
        [JsonProperty("ppvDescriptions")]
        [XmlArray(ElementName = "ppvDescriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("ppv_descriptions")]
        public List<KalturaTranslationToken> PPVDescriptions { get; set; }

        /// <summary>
        /// If the item already purchased - the identifier of the user (in the household) who purchased this item 
        /// </summary>
        [DataMember(Name = "purchaseUserId")]
        [JsonProperty("purchaseUserId")]
        [XmlElement(ElementName = "purchaseUserId")]
        [OldStandardProperty("purchase_user_id")]
        public string PurchaseUserId { get; set; }

        /// <summary>
        /// If the item already purchased - the identifier of the purchased file
        /// </summary>
        [DataMember(Name = "purchasedMediaFileId")]
        [JsonProperty("purchasedMediaFileId")]
        [XmlElement(ElementName = "purchasedMediaFileId")]
        [OldStandardProperty("purchased_media_file_id")]
        public int? PurchasedMediaFileId { get; set; }

        /// <summary>
        /// Related media files identifiers (different types)
        /// </summary>
        [DataMember(Name = "relatedMediaFileIds")]
        [JsonProperty("relatedMediaFileIds")]
        [XmlArray(ElementName = "relatedMediaFileIds", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("related_media_file_ids")]
        public List<KalturaIntegerValue> RelatedMediaFileIds { get; set; }

        /// <summary>
        /// If the item already purchased - since when the user can start watching the item
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        [OldStandardProperty("start_date")]
        public long? StartDate { get; set; }

        /// <summary>
        /// If the item already purchased - until when the user can watch the item
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        [OldStandardProperty("end_date")]
        public long? EndDate { get; set; }

        /// <summary>
        /// Discount end date
        /// </summary>
        [DataMember(Name = "discountEndDate")]
        [JsonProperty("discountEndDate")]
        [XmlElement(ElementName = "discountEndDate")]
        [OldStandardProperty("discount_end_date")]
        public long? DiscountEndDate { get; set; }

        /// <summary>
        /// If the item already purchased and played - the name of the device on which it was first played 
        /// </summary>
        [DataMember(Name = "firstDeviceName")]
        [JsonProperty("firstDeviceName")]
        [XmlElement(ElementName = "firstDeviceName")]
        [OldStandardProperty("first_device_name")]
        public string FirstDeviceName { get; set; }

        /// <summary>
        /// If waiver period is enabled - donates whether the user is still in the cancelation window
        /// </summary>
        [DataMember(Name = "isInCancelationPeriod")]
        [JsonProperty("isInCancelationPeriod")]
        [XmlElement(ElementName = "isInCancelationPeriod")]
        [OldStandardProperty("is_in_cancelation_period")]
        public bool? IsInCancelationPeriod { get; set; }

        /// <summary>
        /// The PPV product code
        /// </summary>
        [DataMember(Name = "ppvProductCode")]
        [JsonProperty("ppvProductCode")]
        [XmlElement(ElementName = "ppvProductCode")]
        [OldStandardProperty("ppv_product_code")]
        public string ProductCode { get; set; }
    }
}