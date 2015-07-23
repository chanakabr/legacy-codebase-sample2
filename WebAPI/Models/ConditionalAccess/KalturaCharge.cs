using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Charge user request 
    /// </summary>
    public class KalturaCharge : KalturaOTTObject
    {
        /// <summary>
        /// User identifier 
        /// </summary>
        [DataMember(Name = "user_id")]
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// Price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        public double Price { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [DataMember(Name = "currency")]
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Coupon code
        /// </summary>
        [DataMember(Name = "coupon_code")]
        [JsonProperty("coupon_code")]
        public string CouponCode { get; set; }

        /// <summary>
        /// Custom extra parameters (changes between different billing providers)
        /// </summary>
        [DataMember(Name = "extra_params")]
        [JsonProperty("extra_params")]
        public string ExtraParams { get; set; }

        /// <summary>
        /// Encrypted credit card CVV
        /// </summary>
        [DataMember(Name = "encrypted_cvv")]
        [JsonProperty("encrypted_cvv")]
        public string EncryptedCvv { get; set; }
    }
}