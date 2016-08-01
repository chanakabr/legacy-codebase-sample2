using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// PPV price details
    /// </summary>
    public class KalturaPpvPrice : KalturaProductPrice
    {
        /// <summary>
        /// Media file identifier  
        /// </summary>
        [DataMember(Name = "fileId")]
        [JsonProperty("fileId")]
        [XmlElement(ElementName = "fileId")]
        public int? FileId { get; set; }

       /// <summary>
        /// The associated PPV module identifier  
        /// </summary>
        [DataMember(Name = "ppvModuleId")]
        [JsonProperty("ppvModuleId")]
        [XmlElement(ElementName = "ppvModuleId")]
        public string PPVModuleId { get; set; }

        /// <summary>
        /// Denotes whether this object is available only as part of a subscription or can be sold separately
        /// </summary>
        [DataMember(Name = "isSubscriptionOnly")]
        [JsonProperty("isSubscriptionOnly")]
        [XmlElement(ElementName = "isSubscriptionOnly")]
        public bool? IsSubscriptionOnly { get; set; }

        /// <summary>
        /// The full price of the item (with no discounts)
        /// </summary>
        [DataMember(Name = "fullPrice")]
        [JsonProperty("fullPrice")]
        [XmlElement(ElementName = "fullPrice", IsNullable = true)]
        public KalturaPrice FullPrice { get; set; }

        /// <summary>
        /// The identifier of the relevant subscription
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        [JsonProperty("subscriptionId")]
        [XmlElement(ElementName = "subscriptionId")]
        public string SubscriptionId { get; set; }

        /// <summary>
        /// The identifier of the relevant collection
        /// </summary>
        [DataMember(Name = "collectionId")]
        [JsonProperty("collectionId")]
        [XmlElement(ElementName = "collectionId")]
        public string CollectionId { get; set; }

        /// <summary>
        /// The identifier of the relevant pre paid
        /// </summary>
        [DataMember(Name = "prePaidId")]
        [JsonProperty("prePaidId")]
        [XmlElement(ElementName = "prePaidId")]
        public string PrePaidId { get; set; }

        /// <summary>
        /// A list of the descriptions of the PPV module on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "ppvDescriptions")]
        [JsonProperty("ppvDescriptions")]
        [XmlArray(ElementName = "ppvDescriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> PPVDescriptions { get; set; }

        /// <summary>
        /// If the item already purchased - the identifier of the user (in the household) who purchased this item 
        /// </summary>
        [DataMember(Name = "purchaseUserId")]
        [JsonProperty("purchaseUserId")]
        [XmlElement(ElementName = "purchaseUserId")]
        public string PurchaseUserId { get; set; }

        /// <summary>
        /// If the item already purchased - the identifier of the purchased file
        /// </summary>
        [DataMember(Name = "purchasedMediaFileId")]
        [JsonProperty("purchasedMediaFileId")]
        [XmlElement(ElementName = "purchasedMediaFileId")]
        public int? PurchasedMediaFileId { get; set; }

        /// <summary>
        /// Related media files identifiers (different types)
        /// </summary>
        [DataMember(Name = "relatedMediaFileIds")]
        [JsonProperty("relatedMediaFileIds")]
        [XmlArray(ElementName = "relatedMediaFileIds", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> RelatedMediaFileIds { get; set; }

        /// <summary>
        /// If the item already purchased - since when the user can start watching the item
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long? StartDate { get; set; }

        /// <summary>
        /// If the item already purchased - until when the user can watch the item
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        public long? EndDate { get; set; }

        /// <summary>
        /// Discount end date
        /// </summary>
        [DataMember(Name = "discountEndDate")]
        [JsonProperty("discountEndDate")]
        [XmlElement(ElementName = "discountEndDate")]
        public long? DiscountEndDate { get; set; }

        /// <summary>
        /// If the item already purchased and played - the name of the device on which it was first played 
        /// </summary>
        [DataMember(Name = "firstDeviceName")]
        [JsonProperty("firstDeviceName")]
        [XmlElement(ElementName = "firstDeviceName")]
        public string FirstDeviceName { get; set; }

        /// <summary>
        /// If waiver period is enabled - donates whether the user is still in the cancelation window
        /// </summary>
        [DataMember(Name = "isInCancelationPeriod")]
        [JsonProperty("isInCancelationPeriod")]
        [XmlElement(ElementName = "isInCancelationPeriod")]
        public bool? IsInCancelationPeriod { get; set; }

        /// <summary>
        /// The PPV product code
        /// </summary>
        [DataMember(Name = "ppvProductCode")]
        [JsonProperty("ppvProductCode")]
        [XmlElement(ElementName = "ppvProductCode")]
        public string ProductCode { get; set; }
    }
}