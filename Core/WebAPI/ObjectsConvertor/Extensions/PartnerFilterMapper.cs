using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Users;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class PartnerFilterMapper
    {
        internal static List<long> GetIdIn(this KalturaPartnerFilter model)
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
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPartnerFilter.idIn");
                    }
                }
            }

            return list;
        }
    }
}
