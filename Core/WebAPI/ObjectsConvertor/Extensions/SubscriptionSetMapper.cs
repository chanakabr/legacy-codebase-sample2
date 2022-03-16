using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SubscriptionSetMapper
    {
        public static List<long> GetSubscriptionIds(this KalturaSubscriptionSet model)
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(model.SubscriptionIds))
            {
                string[] stringValues = model.SubscriptionIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (!long.TryParse(stringValue, out value) || value < 1)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_STRING_CONTAINED_MIN_VALUE_CROSSED, "KalturaSubscriptionSet.subscriptions", 1);
                    }
                    else
                    {
                        list.Add(value);
                    }
                }
            }
            else
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSubscriptionSet.subscriptions");
            }

            return list;
        }
    }
}