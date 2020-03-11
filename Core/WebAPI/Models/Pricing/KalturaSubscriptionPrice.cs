using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscription price details
    /// </summary>
    public partial class KalturaSubscriptionPrice : KalturaProductPrice
    {
        /// <summary>
        /// Product price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        [Obsolete]
        public KalturaPrice Price { get { return base.Price; } set { base.Price = value; }}

        /// <summary>
        /// Product purchase status  
        /// </summary>
        [DataMember(Name = "purchaseStatus")]
        [JsonProperty("purchaseStatus")]
        [XmlElement(ElementName = "purchaseStatus")]
        [OldStandardProperty("purchase_status")]
        [Obsolete]
        public KalturaPurchaseStatus PurchaseStatus { get { return base.PurchaseStatus; } set { base.PurchaseStatus = value; } }

        /// <summary>
        /// If the item related to unified billing cycle purchased - until when the this price is relevant
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        public long? EndDate { get; set; }

    }
}