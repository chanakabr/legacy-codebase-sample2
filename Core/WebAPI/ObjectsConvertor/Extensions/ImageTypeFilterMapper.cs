using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.Catalog.SearchPriorityGroup;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class ImageTypeFilterMapper
    {
        public static List<long> GetIdIn(this KalturaImageTypeFilter model)
        {
            HashSet<long> list = new HashSet<long>();
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
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageTypeFilter.idIn");
                    }
                }
            }

            return new List<long>(list);
        }

        public static List<long> GetRatioIdIn(this KalturaImageTypeFilter model)
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(model.RatioIdIn))
            {
                string[] stringValues = model.RatioIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageTypeFilter.ratioIdIn");
                    }
                }
            }

            return list;
        }
    }
}