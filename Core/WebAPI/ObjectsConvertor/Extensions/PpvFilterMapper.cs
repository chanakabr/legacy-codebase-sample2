using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    internal static class PpvFilterMapper
    {
        public static List<long> GetIdIn(this KalturaPpvFilter model, string field, string ids)
        {
            HashSet<long> list = new HashSet<long>();
            if (!string.IsNullOrEmpty(ids))
            {
                string[] stringValues = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, $"KalturaPpvFilter.{field}");
                    }
                }
            }

            return new List<long>(list);
        }
    }
}
