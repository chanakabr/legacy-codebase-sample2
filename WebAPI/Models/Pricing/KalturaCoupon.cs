using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Coupon details container
    /// </summary>
    [OldStandard("couponsGroup", "coupons_group")]
    public class KalturaCoupon : KalturaOTTObject
    {
        /// <summary>
        /// Coupons group details 
        /// </summary>
        [DataMember(Name = "couponsGroup")]
        [JsonProperty("couponsGroup")]
        [XmlElement(ElementName = "couponsGroup", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaCouponsGroup CouponsGroup { get; set; }

        /// <summary>
        /// Coupon status 
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaCouponStatus Status { get; set; }
    }
}