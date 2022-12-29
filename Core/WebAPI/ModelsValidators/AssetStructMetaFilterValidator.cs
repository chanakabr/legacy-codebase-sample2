using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class AssetStructMetaFilterValidator
    {
        public static void Validate(this KalturaAssetStructMetaFilter model)
        {
            if (!model.AssetStructIdEqual.HasValue && !model.MetaIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "AssetStructIdEqual", "MetaIdEqual");
            }
        }
    }
}
