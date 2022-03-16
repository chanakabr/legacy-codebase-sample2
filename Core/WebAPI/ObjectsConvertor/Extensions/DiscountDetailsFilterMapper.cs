using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class DiscountDetailsFilterMapper
    {
        public static List<long> GetIdIn(this KalturaDiscountDetailsFilter model)
        {
            List<long> list = null;
            if (!string.IsNullOrEmpty(model.IdIn))
            {
                list = new List<long>();
                string[] stringValues = model.IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                long longValue;
                foreach (string stringValue in stringValues)
                {
                    if (long.TryParse(stringValue, out longValue))
                    {
                        list.Add(longValue);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaDiscountFilter.codeIn");
                    }
                }
            }

            return list;
        }
    }
}