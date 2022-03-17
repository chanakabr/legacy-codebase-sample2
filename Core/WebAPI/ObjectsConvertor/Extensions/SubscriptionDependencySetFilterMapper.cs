using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SubscriptionDependencySetFilterMapper
    {
        public static List<long> GetBaseSubscriptionIdContains(this KalturaSubscriptionDependencySetFilter model)
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(model.BaseSubscriptionIdIn))
            {
                string[] stringValues =
                    model.BaseSubscriptionIdIn.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT,
                            "KalturaSubscriptionDependencySetFilter.BaseSubscriptionIdIn");
                    }
                }
            }

            return list;
        }
    }
}