using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Pricing;
using Core.Pricing.Handlers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.Domains
{
    // TODO SHIR - CRUD changes
    /// <summary>
    /// Household Coupon details
    /// </summary>
    public partial class KalturaHouseholdCoupon : KalturaCrudObject<CouponWallet, string, CouponWalletFilter>
    {        
        /// <summary>
        /// Coupon code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Coupon 
        /// </summary>
        [DataMember(Name = "coupon")]
        [JsonProperty("coupon")]
        [XmlElement(ElementName = "coupon", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaCoupon Coupon{ get; set; }

        internal override ICrudHandler<CouponWallet, string, CouponWalletFilter> Handler
        {
            get
            {
                return CouponWalletHandler.Instance;
            }
        }

        internal override void ValidateForAdd()
        {
            if (string.IsNullOrEmpty(this.Code))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "code");
            }
        }

        internal override void ValidateForUpdate()
        {
            throw new System.NotImplementedException();
        }
    }

    public partial class KalturaHouseholdCouponListResponse : KalturaListResponse
    {
        /// <summary>
        /// Household coupon
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHouseholdCoupon> Objects { get; set; }
    }
}