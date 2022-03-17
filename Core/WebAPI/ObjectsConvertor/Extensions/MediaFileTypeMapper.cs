using System;
using System.Collections.Generic;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class MediaFileTypeMapper
    {
        public static HashSet<string> CreateMappedHashSetForMediaFileType(this KalturaMediaFileType model, string codecs)
        {
            HashSet<string> result = null;
            if (!string.IsNullOrEmpty(codecs))
            {
                string[] codecValues = codecs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (codecValues != null && codecValues.Length > 0)
                {
                    result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string codec in codecValues)
                    {
                        if (!result.Contains(codec))
                        {
                            result.Add(codec);
                        }
                    }
                }
            }

            return result;
        }
    }
}