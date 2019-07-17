using ApiLogic.Base;
using ApiObjects.Pricing;
using Core.Pricing.Handlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    [Serializable]
    public partial class KalturaHouseholdCouponFilter : KalturaCrudFilter<KalturaHouseholdCouponOrderBy, CouponWallet, string, CouponWalletFilter>
    {
        /// <summary>
        /// Indicates which household coupons list to return by their business module type.
        /// </summary>
        [DataMember(Name = "businessModuleTypeEqual")]
        [JsonProperty("businessModuleTypeEqual")]
        [XmlElement(ElementName = "businessModuleTypeEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaTransactionType BusinessModuleTypeEqual { get; set; }

        /// <summary>
        /// Indicates which household coupons list to return by their business module ID.
        /// </summary>
        [DataMember(Name = "businessModuleIdEqual")]
        [JsonProperty("businessModuleIdEqual")]
        [XmlElement(ElementName = "businessModuleIdEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? BusinessModuleIdEqual { get; set; }

        internal override ICrudHandler<CouponWallet, string, CouponWalletFilter> Handler
        {
            get
            {
                return CouponWalletHandler.Instance;
            }
        }

        public override KalturaHouseholdCouponOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdCouponOrderBy.NONE;
        }
    }

    public enum KalturaHouseholdCouponOrderBy
    {
        NONE
    }
}