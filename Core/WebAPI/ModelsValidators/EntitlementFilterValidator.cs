using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class BaseEntitlementFilterValidator
    {
        internal static void Validate(this KalturaBaseEntitlementFilter model)
        {
            switch (model)
            {
                case KalturaEntitlementFilter f: f.Validate(); break;
                case KalturaProgramAssetGroupOfferEntitlementFilter f: 
                default:
                    break;
            }
        }
    }

    public static class EntitlementFilterValidator
    {
        internal static void Validate(this KalturaEntitlementFilter model)
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
    }
}