using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Billing
{
    public class PaymentGWHouseholdResponse
    {
        /// <summary>
        /// Billing account identifier 
        /// </summary>
        [DataMember(Name = "charge_id")]
        [JsonProperty("charge_id")]
        public string  chargeID { get; set; }
    }
}