using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AssetStructMapper
    {
        internal static HashSet<string> GetFeaturesAsHashSet(this KalturaAssetStruct model)
        {
            if (model.Features == null)
            {
                return null;
            }

            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] splitedFeatures = model.Features.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string feature in splitedFeatures)
            {
                if (result.Contains(feature))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "KalturaAssetStruct.features");
                }
                else
                {
                    result.Add(feature);
                }
            }

            return result;
        }
    }
}
