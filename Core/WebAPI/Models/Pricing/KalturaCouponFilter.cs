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
    [Serializable]
    public partial class KalturaCouponFilter : KalturaFilter<KalturaCouponFilterOrderBy>
    {
        /// <summary>
        /// Comma separated list of coupon codes.
        /// </summary>
        [DataMember(Name = "couponCodesIn")]
        [JsonProperty("couponCodesIn")]
        [XmlElement(ElementName = "couponCodesIn")]
        public string CouponCodesIn { get; set; }

        public override KalturaCouponFilterOrderBy GetDefaultOrderByValue()
        {
            return KalturaCouponFilterOrderBy.NONE;
        }
    }
}