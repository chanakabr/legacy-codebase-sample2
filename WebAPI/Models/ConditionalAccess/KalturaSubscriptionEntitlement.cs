using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// KalturaSubscriptionEntitlement
    /// </summary>   
    public class KalturaSubscriptionEntitlement : KalturaEntitlement
    {

        /// <summary>
        ///The date of the next renewal (only for subscription)
        /// </summary>
        [DataMember(Name = "nextRenewalDate")]
        [JsonProperty("nextRenewalDate")]
        [XmlElement(ElementName = "nextRenewalDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? NextRenewalDate { get; set; }

        /// <summary>
        ///Indicates whether the subscription is renewable in this purchase (only for subscription)
        /// </summary>
        [DataMember(Name = "isRenewableForPurchase")]
        [JsonProperty("isRenewableForPurchase")]
        [XmlElement(ElementName = "isRenewableForPurchase")]
        [SchemeProperty(ReadOnly = true)]
        public bool? IsRenewableForPurchase { get; set; }

        /// <summary>
        ///Indicates whether a subscription is renewable (only for subscription)
        /// </summary>
        [DataMember(Name = "isRenewable")]
        [JsonProperty("isRenewable")]
        [XmlElement(ElementName = "isRenewable")]
        [SchemeProperty(ReadOnly = true)]
        public bool? IsRenewable { get; set; }

        /// <summary>
        /// Indicates whether the user is currently in his grace period entitlement
        /// </summary>
        [DataMember(Name = "isInGracePeriod")]
        [JsonProperty("isInGracePeriod")]
        [XmlElement(ElementName = "isInGracePeriod")]
        [SchemeProperty(ReadOnly = true)]
        public bool? IsInGracePeriod { get; set; }

        /// <summary>
        ///Payment Gateway identifier
        /// </summary>
        [DataMember(Name = "paymentGatewayId")]
        [JsonProperty("paymentGatewayId")]
        [XmlElement(ElementName = "paymentGatewayId")]
        [SchemeProperty(MinInteger = 1)]
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        ///Payment Method identifier
        /// </summary>
        [DataMember(Name = "paymentMethodId")]
        [JsonProperty("paymentMethodId")]
        [XmlElement(ElementName = "paymentMethodId")]
        [SchemeProperty(MinInteger = 1)]
        public int? PaymentMethodId { get; set; }
    }
}