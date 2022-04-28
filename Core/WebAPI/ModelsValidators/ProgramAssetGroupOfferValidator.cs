using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class ProgramAssetGroupOfferValidator
    {
        internal static void ValidateForAdd(this KalturaProgramAssetGroupOffer model)
        {
            if (model.Name == null || model.Name.Values == null || model.Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
            model.Name.Validate("multilingualName");

            if (model.PriceDetailsId == null)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "priceDetailsId");

            if (!model.StartDate.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "startDate");
            }

            if (!model.ExpiryDate.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "expiryDate");
            }

            if (!model.EndDate.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "endDate");
            }

            if (model.StartDate >= model.EndDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDate", "endDate");
            }

            if (model.StartDate > model.ExpiryDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDate", "expiryDate");
            }

            if (model.EndDate > model.ExpiryDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "expiryDate", "endDate");
            }

            if (string.IsNullOrEmpty(model.ExternalOfferId))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalOfferId");
        }

        internal static void ValidateForUpdate(this KalturaProgramAssetGroupOffer model)
        {
            if (model.Name != null)
            {
                if (model.Name.Values.Count > 0 && string.IsNullOrEmpty(model.Name.Values[0].Value))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }
                model.Name.Validate("multilingualName");
            }

            if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDate", "endDate");
            }

            if (model.StartDate.HasValue && model.ExpiryDate.HasValue && model.StartDate > model.ExpiryDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDate", "expiryDate");
            }

            if (model.ExpiryDate.HasValue && model.EndDate.HasValue && model.EndDate > model.ExpiryDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "expiryDate", "endDate");
            }
        }
    }
}