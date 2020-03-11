using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Pricing;
using Core.Pricing.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household Coupon details
    /// </summary>
    [Serializable]
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
        /// Last Usage Date
        /// </summary>
        [DataMember(Name = "lastUsageDate")]
        [JsonProperty("lastUsageDate")]
        [XmlElement(ElementName = "lastUsageDate")]
        public long? LastUsageDate { get; set; }

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
        public KalturaHouseholdCouponListResponse() : base() { }
        
        public override void SetRelatedObjects(ContextData contextData, KalturaDetachedResponseProfile profile)
        {
            if (Objects != null && Objects.Count > 0)
            {
                foreach (var householdCoupon in Objects)
                {
                    var res = PricingUtils.GetCouponListResponse(contextData, householdCoupon);
                    if (res != null)
                    {
                        if (householdCoupon.relatedObjects == null)
                        {
                            householdCoupon.relatedObjects = new SerializableDictionary<string, IKalturaListResponse>();
                        }

                        if (!householdCoupon.relatedObjects.ContainsKey(profile.Name))
                        {
                            householdCoupon.relatedObjects.Add(profile.Name, res);
                        }
                    }
                }
            }
        }
    }
}