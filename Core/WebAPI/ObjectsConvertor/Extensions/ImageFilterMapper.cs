using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class ImageFilterMapper
    {
        public static List<long> GetIdIn(this KalturaImageFilter model)
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
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageFilter.idIn");
                    }
                }
            }

            return new List<long>(list);
        }

        public static List<long> GetImageObjectIdIn(this KalturaImageFilter model)
        {
            HashSet<long> list = new HashSet<long>();
            if (!string.IsNullOrEmpty(model.ImageObjectIdIn))
            {
                string[] stringValues = model.ImageObjectIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageFilter.imageObjectIdIn");
                    }
                }
            }

            return new List<long>(list);
        }
    }
}