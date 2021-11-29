using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess.FilterActions.Assets;

namespace WebAPI.ModelsValidators
{
    public static class FilterAssetByKsqlActionValidator
    {
        public static void Validate(this KalturaFilterAssetByKsqlAction model)
        {
            if (string.IsNullOrWhiteSpace(model.Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
            }
        }
    }
}