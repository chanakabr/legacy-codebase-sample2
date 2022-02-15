using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AssetHistoryFilterMapper
    {
        public static int getDaysLessThanOrEqual(this KalturaAssetHistoryFilter model)
        {
            return model.DaysLessThanOrEqual.HasValue ? model.DaysLessThanOrEqual.Value : 0;
        }

        public static List<int> getTypeIn(this KalturaAssetHistoryFilter model)
        {
            if (model.filterTypes != null)
                return model.filterTypes.Select(x => x.value).ToList();

            if (string.IsNullOrEmpty(model.TypeIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = model.TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetHistoryFilter.typeIn");
                }
            }

            return values;
        }

        public static List<string> getAssetIdIn(this KalturaAssetHistoryFilter model)
        {
            if (model.AssetIdIn == null)
                return null;

            return model.AssetIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
