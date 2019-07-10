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
    // TODO ANAT(BEO-6931) - ADD ALL relevant KalturaHouseholdCoupon data members and validation

    /// <summary>
    /// Household Coupon details
    /// </summary>
    public partial class KalturaHouseholdCoupon : KalturaOTTObject, IKalturaCrudHandeledObject<CouponWalt>
    {
        private static readonly CouponWaltHandler couponWaltHandler = new CouponWaltHandler();
        
        /// <summary>
        /// Coupon code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        public ICrudHandler<CouponWalt> GetHandler()
        {
            return couponWaltHandler;
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