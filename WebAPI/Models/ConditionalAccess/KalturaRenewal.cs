using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaEntitlementRenewal : KalturaOTTObject
    {
        /// <summary>
        /// Price that is going to be paid on the renewal
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price")]
        public KalturaPrice Price { get; set; }

        /// <summary>
        /// Next renewal date
        /// </summary>
        [DataMember(Name = "date")]
        [JsonProperty("date")]
        [XmlElement(ElementName = "date")]
        public long Date { get; set; }

        /// <summary>
        /// Puchase ID
        /// </summary>
        [DataMember(Name = "purchaseId")]
        [JsonProperty("purchaseId")]
        [XmlElement(ElementName = "purchaseId")]
        public long PurchaseId { get; set; }
        
        /// <summary>
        /// Subscription ID
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        [JsonProperty("subscriptionId")]
        [XmlElement(ElementName = "subscriptionId")]
        public long SubscriptionId { get; set; }
    }

    public class KalturaUnifiedPaymentRenewal : KalturaOTTObject
    {
        /// <summary>
        /// Price that is going to be paid on the renewal
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price")]
        public KalturaPrice Price { get; set; }

        /// <summary>
        /// Next renewal date
        /// </summary>
        [DataMember(Name = "date")]
        [JsonProperty("date")]
        [XmlElement(ElementName = "date")]
        public long Date { get; set; }

        /// <summary>
        /// Unified payment ID
        /// </summary>
        [DataMember(Name = "unifiedPaymentId")]
        [JsonProperty("unifiedPaymentId")]
        [XmlElement(ElementName = "unifiedPaymentId")]
        public long UnifiedPaymentId { get; set; }

        /// <summary>
        /// List of entitlements in this unified payment renewal
        /// </summary>
        [DataMember(Name = "entitlements")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaEntitlementRenewalBase> Entitlements { get; set; }
    }

    public class KalturaEntitlementRenewalBase : KalturaOTTObject
    {
        /// <summary>
        /// Price that is going to be paid on the renewal
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price")]
        public double Price { get; set; }

        /// <summary>
        /// Puchase ID
        /// </summary>
        [DataMember(Name = "purchaseId")]
        [JsonProperty("purchaseId")]
        [XmlElement(ElementName = "purchaseId")]
        public long PurchaseId { get; set; }

        /// <summary>
        /// Subscription ID
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        [JsonProperty("subscriptionId")]
        [XmlElement(ElementName = "subscriptionId")]
        public long SubscriptionId { get; set; }
    }
}