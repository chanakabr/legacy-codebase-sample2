using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Coupon details container
    /// </summary>
    public class KalturaCouponDetails
    {
        /// <summary>
        /// Coupons group details 
        /// </summary>
        [DataMember(Name = "coupons_group")]
        [JsonProperty("coupons_group")]
        public KalturaCouponsGroup CouponsGroup { get; set; }

        /// <summary>
        /// Coupon status 
        /// </summary>
        [DataMember(Name = "coupon_status")]
        [JsonProperty("coupon_status")]
        public KalturaCouponStatus CouponStatus { get; set; }
    }
}