using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;


namespace WebAPI.Models.Billing
{
    /// <summary>
    /// Payment Gateway Settings
    /// </summary>
    public class KalturaPaymentGWSettingsResponse
    {
        /// <summary>
        /// List of payment_gateway_settings
        /// </summary>
        [DataMember(Name = "payment_gateway")]
        [JsonProperty("payment_gateway")]
        public List<KalturaPaymentGW> pgw { get; set; }
      
    }
}