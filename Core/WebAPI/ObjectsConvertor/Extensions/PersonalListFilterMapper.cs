using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Api;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class PersonalListFilterMapper
    {
        public static  HashSet<int> GetPartnerListTypeIn(this KalturaPersonalListFilter model)
        {
            if (string.IsNullOrEmpty(model.PartnerListTypeIn))
                return null;

            HashSet<int> values = new HashSet<int>();
            string[] stringValues = model.PartnerListTypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPersonalListFilter.PartnerListTypeIn");
                }
            }

            return values;
        }
    }
}