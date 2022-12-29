using Phx.Lib.Appconfig;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.ModelsValidators
{
    public static class AssetStructValidator
    {
        public static bool Validate(this KalturaAssetStruct model)
        {
            // validate metaIds
            if (!string.IsNullOrEmpty(model.MetaIds))
            {
                string[] stringValues = model.MetaIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (!long.TryParse(stringValue, out value) || value < 1)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetStruct.metaIds");
                    }
                }
            }

            // validate features
            if (!string.IsNullOrEmpty(model.Features))
            {
                HashSet<string> featuresHashSet = model.GetFeaturesAsHashSet();
                if (featuresHashSet != null && featuresHashSet.Count > 0)
                {
                    string allowedPattern = ApplicationConfiguration.Current.MetaFeaturesPattern.Value;
                    Regex regex = new Regex(allowedPattern);
                    foreach (string feature in featuresHashSet)
                    {
                        if (regex.IsMatch(feature))
                        {
                            throw new BadRequestException(BadRequestException.INVALID_VALUE_FOR_FEATURE, feature);
                        }
                    }
                }
            }

            if (model.DynamicData != null)
            {
                foreach (var data in model.DynamicData)
                {
                    if (data.Value.value == null)
                    {
                        model.DynamicData.Remove(data.Key);
                    }
                }
            }

            return true;
        }
    }
}
