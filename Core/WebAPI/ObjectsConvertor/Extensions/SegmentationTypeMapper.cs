using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Segmentation;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SegmentationTypeMapper
    {
        public static HashSet<long> GetIdIn(this KalturaSegmentationTypeFilter model)
        {
            HashSet<long> hashSet = new HashSet<long>();
            if (!string.IsNullOrEmpty(model.IdIn))
            {
                string[] stringValues = model.IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !hashSet.Contains(value))
                    {
                        hashSet.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSegmentationTypeFilter.idIn");
                    }
                }
            }

            return hashSet;
        }

        public static List<long> GetIdIn(this KalturaSegmentValueFilter model)
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(model.IdIn))
            {
                string[] stringValues = model.IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "idIn");
                    }
                }
            }

            return list;
        }
    }

    public static class MonetizationConditionMapper 
    {
        public static List<int> GetBusinessModuleIdIn(this KalturaMonetizationCondition model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(model.BusinessModuleIdIn, "KalturaMonetizationCondition.businessModuleIdIn");
        }
    }
}
