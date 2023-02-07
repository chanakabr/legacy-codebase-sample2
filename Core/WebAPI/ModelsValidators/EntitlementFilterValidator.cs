using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class EntitlementFilterValidator
    {
        internal static void Validate(this KalturaEntitlementFilter model)
        {
            switch (model)
            {
                case KalturaProgramAssetGroupOfferEntitlementFilter f: f.ValidateProgramAssetGroupOfferEntitlementFilter(); break;
                case KalturaEntitlementFilter f: f.ValidateEntitlementFilter(); break;
                default: break;
            }
        }

        private static void ValidateEntitlementFilter(this KalturaEntitlementFilter model)
        {
            if (!model.ProductTypeEqual.HasValue && !model.EntitlementTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaEntitlementFilter.productTypeEqual");
            }

            if (model.ProductTypeEqual.HasValue && model.ProductTypeEqual == KalturaTransactionType.programAssetGroupOffer)
            {
                throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "KalturaEntityReferenceBy.productTypeEqual", model.ProductTypeEqual);
            }
        }

        private static void ValidateProgramAssetGroupOfferEntitlementFilter(this KalturaProgramAssetGroupOfferEntitlementFilter model)
        {
            if (model.EntitlementTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "entitlementTypeEqual");
            }

            if (model.ProductTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "productTypeEqual");
            }

            if (model.IsExpiredEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "isExpiredEqual");
            }
        }
    }
}