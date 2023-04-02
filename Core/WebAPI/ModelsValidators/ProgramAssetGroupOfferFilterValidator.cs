using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class ProgramAssetGroupOfferFilterValidator
    {
        public static void Validate(this KalturaProgramAssetGroupOfferFilter model)
        {
            switch (model)
            {
                case KalturaProgramAssetGroupOfferIdInFilter f:
                    f.Validate();
                    break;
                case KalturaProgramAssetGroupOfferFilter f:
                    break;
                default:
                    break;
            }
        }

        public static void Validate(this KalturaProgramAssetGroupOfferIdInFilter model)
        {
            if (!string.IsNullOrEmpty(model.IdIn) && !string.IsNullOrEmpty(model.NameContains))
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaProgramAssetGroupOfferIdInFilter.nameContains", "KalturaProgramAssetGroupOfferIdInFilter.idIn");
        }
    }
}