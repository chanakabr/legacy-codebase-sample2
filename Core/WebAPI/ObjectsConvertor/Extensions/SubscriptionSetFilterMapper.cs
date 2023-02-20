using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SubscriptionSetFilterMapper
    {
        public static List<long> GetIdIn(this KalturaSubscriptionSetFilter model)
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(model.IdIn))
            {
                string[] stringValues = model.IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSubscriptionSetFilter.idIn");
                    }
                }
            }

            return list;
        }

        public static List<long> GetSubscriptionIdContains(this KalturaSubscriptionSetFilter model)
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(model.SubscriptionIdContains))
            {
                string[] stringValues = model.SubscriptionIdContains.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSubscriptionSetFilter.SubscriptionIdContains");
                    }
                }
            }

            return list;
        }
    }
}
