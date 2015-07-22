using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// Payment Gateway
    /// </summary>
    public class KalturaPaymentGWResponse
    {
        /// <summary>
        /// List of payment_gateway
        /// </summary>
        [DataMember(Name = "payment_gateway_basic")]
        [JsonProperty("payment_gateway_basic")]
        public List<KalturaPaymentGWBasic> pgw { get; set; }
    }
}