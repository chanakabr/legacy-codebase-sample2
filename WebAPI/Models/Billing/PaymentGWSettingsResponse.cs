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
    public class PaymentGWSettingsResponse
    {
        /// <summary>
        /// List of payment_gateway_settings
        /// </summary>
        [DataMember(Name = "payment_gateway")]
        [JsonProperty("payment_gateway")]
        public List<PaymentGW> pgw { get; set; }
      
    }
}