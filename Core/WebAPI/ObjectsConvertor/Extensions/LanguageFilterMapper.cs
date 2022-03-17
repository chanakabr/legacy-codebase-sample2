using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class LanguageFilterMapper
    {
        public static List<string> GetCodeIn(this KalturaLanguageFilter model)
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(model.CodeIn))
            {
                string[] stringValues = model.CodeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string languageCode in stringValues)
                {
                    if (!string.IsNullOrEmpty(languageCode))
                    {
                        list.Add(languageCode);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaLanguageFilter.CodeIn");
                    }
                }
            }

            return list;
        }
    }
}