using ApiObjects;
using ApiObjects.Response;
using Core.ConditionalAccess;
using Core.Pricing;
using System;
using System.Collections.Generic;

namespace ApiLogic.Pricing
{
    public class PromotionEvaluator
    {
        private readonly int _groupId;
        private readonly string _countryCode;
        private readonly string _currencyCode;
        private readonly string _couponCode;
        private readonly Price _originalPrice;
        private readonly int _domainId;
        private readonly IPricingModule _pricingModule;
        private readonly IConditionalAccessUtils _conditionalAccessUtils;

        public PromotionEvaluator(IPricingModule pricingModule, IConditionalAccessUtils conditionalAccessUtils, int groupId, int domainId, string countryCode, 
                                  string currencyCode, string couponCode, Price originalPrice)
        {
            _pricingModule = pricingModule;
            _conditionalAccessUtils = conditionalAccessUtils;
            _groupId = groupId;
            _domainId = domainId;
            _countryCode = countryCode;
            _currencyCode = currencyCode;
            _couponCode = couponCode;
            _originalPrice = originalPrice;
        }

        public Price Evaluate(BasePromotion promotion)
        {
            switch (promotion)
            {
                case Promotion p: return EvaluatePromotion(p);
                case CouponPromotion p: return EvaluatePromotion(p);
                default: throw new NotImplementedException($"Evaluation for promotion type {promotion.GetType().Name} was not implemented in PromotionEvaluator");
            }
        }

        private Price EvaluatePromotion(Promotion promotion)
        {
            var discountModule = _pricingModule.GetDiscountCodeDataByCountryAndCurrency(_groupId, (int)(promotion.DiscountModuleId), _countryCode, _currencyCode);
            if (discountModule == null) { return null; }
            var price = _conditionalAccessUtils.GetPriceAfterDiscount(_originalPrice, discountModule, 0);
            return price;
        }

        private Price EvaluatePromotion(CouponPromotion promotion)
        {
            if (string.IsNullOrEmpty(_couponCode)) { return null; }

            var couponsGroupResponse = _pricingModule.GetCouponsGroup(_groupId, promotion.CouponGroupId);
            if (!Status.Ok.Equals(couponsGroupResponse.Status)) { return null; }
            var couponsGroup = couponsGroupResponse.CouponsGroup;

            List<SubscriptionCouponGroup> subscriptionCouponGroups = null;
            string couponCode = _couponCode;
            var price = _conditionalAccessUtils.GetLowestPriceByCouponCode(_groupId, ref couponCode, subscriptionCouponGroups, _originalPrice, _domainId, 
                couponsGroup, _countryCode);
            return price;
        }
    }
}
