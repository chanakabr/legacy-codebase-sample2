using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class IngestProfileMapper
    {
        public static List<int> GetOverlapChannels(this KalturaIngestProfile model)
        {
            List<int> list = new List<int>();

            if (!string.IsNullOrEmpty(model.OverlapChannels))
            {
                string[] stringValues = model.OverlapChannels.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string value in stringValues)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        list.Add(int.Parse(value));
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaLanguageFilter.OverlapChannels");
                    }
                }
            }

            return list;
        }
    }
}