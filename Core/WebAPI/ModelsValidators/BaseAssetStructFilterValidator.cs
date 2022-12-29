using System;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.ModelsValidators
{
    public static class BaseAssetStructFilterValidator
    {
        internal static void Validate(this KalturaBaseAssetStructFilter model)
        {
            switch (model)
            {
                case KalturaLinearAssetStructFilter _: break;
                case KalturaAssetStructFilter c: c.Validate(); break ;
                default: throw new NotImplementedException($"Validate for {model.objectType} is not implemented");
            }
        }
    }

    public static class AssetStructFilterValidator
    {
        internal static void Validate(this KalturaAssetStructFilter model)
        {
            if (!string.IsNullOrEmpty(model.IdIn) && model.MetaIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaAssetStructFilter.idIn", "KalturaAssetStructFilter.metaIdEqual");
            }

            if (!string.IsNullOrEmpty(model.IdIn))
            {
                // GetAssetStructIds does parsing and validation (throws BadRequest).
                // Ideally it should be just validation here, but as result is available then we could save and use it.
                model.GetAssetStructIds();
            }
        }
    }
}
