using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// PaymentGWBasic
    /// </summary>
    public class KalturaPaymentGWBasic : KalturaOTTObject
    {
        /// <summary>
        /// payment gateway id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public int id { get; set; }

        /// <summary>
        /// payment gateway name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string name { get; set; }
    }
}
