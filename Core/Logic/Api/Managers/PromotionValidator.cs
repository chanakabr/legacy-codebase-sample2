using ApiObjects;
using ApiObjects.Response;
using Core.Pricing;
using System;
using System.Linq;
using System.Threading;

namespace ApiLogic.Api.Managers
{
    public interface IPromotionValidator
    {
        Status Validate(int groupId, BasePromotion promotion);
    }
    public class PromotionValidator : IPromotionValidator
    {
        private static readonly Lazy<PromotionValidator> LazyInstance = new Lazy<PromotionValidator>(() =>
            new PromotionValidator(Module.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IPromotionValidator Instance => LazyInstance.Value;

        private readonly IPricingModule _pricing;

        public PromotionValidator(IPricingModule pricing)
        {
            _pricing = pricing;
        }

        public Status Validate(int groupId, BasePromotion promotion)
        {
            switch (promotion)
            {
                case Promotion p: return Validate(groupId, p);
                case CouponPromotion p: return Validate(groupId, p);
                default: throw new NotImplementedException($"Validation for Promotion type {this.GetType().Name} was not implemented in promotionValidator");
            }
        }

        private Status Validate(int groupId, Promotion promotion)
        {
            var discounts = _pricing.GetValidDiscounts(groupId);
            if (!discounts.HasObjects() || !discounts.Objects.Any(x => x.Id == promotion.DiscountModuleId))
            {
                return new Status(eResponseStatus.DiscountCodeNotExist);
            }

            return new Status(eResponseStatus.OK);
        }

        private Status Validate(int groupId, CouponPromotion promotion)
        {
            var result = _pricing.GetCouponsGroup(groupId, promotion.CouponGroupId);

            if (!result.Status.IsOkStatusCode() || result.CouponsGroup == null)
            {
                return result.Status;
            }

            return new Status(eResponseStatus.OK);
        }
    }
}