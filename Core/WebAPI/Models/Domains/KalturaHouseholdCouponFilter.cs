using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
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
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.Domains
{
    [Serializable]
    public partial class KalturaHouseholdCouponFilter : KalturaCrudFilter<KalturaHouseholdCouponOrderBy, CouponWallet>
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
        [SchemeProperty(MinInteger = 1)]
        public long BusinessModuleIdEqual { get; set; }

        /// <summary>
        /// Allow clients to inquiry if a specific coupon is part of an HH’s wallet or not
        /// </summary>
        [DataMember(Name = "couponCode")]
        [JsonProperty("couponCode")]
        [XmlElement(ElementName = "couponCode", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(MinLength = 1)]
        public string CouponCode { get; set; }
        /// <summary>
        /// Allow clients to filter out coupons which are valid/invalid
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaCouponStatus? Status { get; set; }

        private static readonly Type relatedObjectFilterType = typeof(KalturaHouseholdCouponCodeFilter);

        public override Type RelatedObjectFilterType
        {
            get
            {
                return relatedObjectFilterType;
            }
        }

        public override KalturaHouseholdCouponOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdCouponOrderBy.NONE;
        }

        public override void Validate()
        {
            if (BusinessModuleIdEqual == 0 && string.IsNullOrEmpty(CouponCode) && Status == null)
            {
                var filterName = "KalturaHouseholdCouponFilter";
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, $"{filterName}.businessModuleIdEqual, {filterName}.couponCode, {filterName}.status");
            }
        }

        public override GenericListResponse<CouponWallet> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CouponWalletFilter>(this);
            return CouponWalletHandler.Instance.List(contextData, coreFilter);
        }

        public KalturaHouseholdCouponFilter() : base()
        {
        }
    }

    public enum KalturaHouseholdCouponOrderBy
    {
        NONE
    }
}