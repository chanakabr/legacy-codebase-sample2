using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Pricing;
using Core.Pricing.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.Domains
{
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

        internal override void SetId(string id)
        {
            this.Code = id;
        }

        public KalturaHouseholdCoupon() : base() { }
    }

    public partial class KalturaHouseholdCouponListResponse : KalturaListResponse<KalturaHouseholdCoupon>
    {
        /// <summary>
        /// Household coupon
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHouseholdCoupon> Objects { get; set; }

        internal override void SetData(KalturaGenericListResponse<KalturaHouseholdCoupon> kalturaGenericList)
        {
            this.Objects = kalturaGenericList.Objects;
            this.TotalCount = kalturaGenericList.TotalCount;
        }

        public KalturaHouseholdCouponListResponse(): base()
        {
        }
    }
}