using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaSubscriptionEntitlement : KalturaEntitlement
    {
        /// <summary>
        ///Payment Gateway identifier (only for Subscription)
        /// </summary>
        [DataMember(Name = "paymentGatewayId")]
        [JsonProperty("paymentGatewayId")]
        [XmlElement(ElementName = "paymentGatewayId")]       
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        ///Payment Method identifier (only for Subscription)
        /// </summary>
        [DataMember(Name = "paymentMethodId")]
        [JsonProperty("paymentMethodId")]
        [XmlElement(ElementName = "paymentMethodId")]
        public int? PaymentMethodId { get; set; }
    }
}