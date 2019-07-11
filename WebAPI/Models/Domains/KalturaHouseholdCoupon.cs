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

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household Coupon details
    /// </summary>
    public partial class KalturaHouseholdCoupon : KalturaOTTObject, IKalturaCrudHandeledObject<CouponWallet>
    {
        private static readonly CouponWalletHandler couponWalletHandler = new CouponWalletHandler();
        
        /// <summary>
        /// Coupon code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        public ICrudHandler<CouponWallet> GetHandler()
        {
            return couponWalletHandler;
        }

        public void ValidateForAdd()
        {
            if (string.IsNullOrEmpty(this.Code))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "code");
            }
        }

        public void ValidateForUpdate()
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