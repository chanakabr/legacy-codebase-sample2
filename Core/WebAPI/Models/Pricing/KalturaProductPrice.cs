using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    [XmlInclude(typeof(KalturaItemPrice))]
    [XmlInclude(typeof(KalturaPpvPrice))]
    [XmlInclude(typeof(KalturaSubscriptionPrice))]
    [XmlInclude(typeof(KalturaCollectionPrice))]
    abstract public partial class KalturaProductPrice : KalturaOTTObject
    {
        /// <summary>
        /// Product identifier
        /// </summary>
        [DataMember(Name = "productId")]
        [JsonProperty("productId")]
        [XmlElement(ElementName = "productId")]
        [OldStandardProperty("product_id")]
        public string ProductId { get; set; }

        /// <summary>
        /// Product Type
        /// </summary>
        [DataMember(Name = "productType")]
        [JsonProperty("productType")]
        [XmlElement(ElementName = "productType")]
        [OldStandardProperty("product_type")]
        public KalturaTransactionType ProductType { get; set; }

        /// <summary>
        /// Product price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        [OnlyNewStandard]
        public KalturaPrice Price { get; set; }

        /// <summary>
        /// The full price of the item (with no discounts)
        /// </summary>
        [DataMember(Name = "fullPrice")]
        [JsonProperty("fullPrice")]
        [XmlElement(ElementName = "fullPrice", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaPrice FullPrice { get; set; }

        /// <summary>
        /// Product purchase status  
        /// </summary>
        [DataMember(Name = "purchaseStatus")]
        [JsonProperty("purchaseStatus")]
        [XmlElement(ElementName = "purchaseStatus")]
        [OnlyNewStandard]
        public KalturaPurchaseStatus PurchaseStatus { get; set; }

        /// <summary>
        /// Promotion Info
        /// </summary>
        [DataMember(Name = "promotionInfo")]
        [JsonProperty("promotionInfo")]
        [XmlElement(ElementName = "promotionInfo")]
        [SchemeProperty(IsNullable = true)]
        public KalturaPromotionInfo PromotionInfo { get; set; }
    }
}