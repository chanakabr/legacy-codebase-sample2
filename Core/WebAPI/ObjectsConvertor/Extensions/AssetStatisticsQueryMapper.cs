using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AssetStatisticsQueryMapper
    {
        internal static List<int> getAssetIdIn(this KalturaAssetStatisticsQuery model)
        {
            if (string.IsNullOrEmpty(model.AssetIdIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = model.AssetIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetStatisticsQuery.assetIdIn");
                }
            }

            return values;
        }
    }
}
