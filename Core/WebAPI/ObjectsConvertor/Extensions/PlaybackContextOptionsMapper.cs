using System;
using System.Collections.Generic;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class PlaybackContextOptionsMapper
    {
        public static List<long> GetMediaFileIds(this KalturaPlaybackContextOptions model)
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(model.AssetFileIds))
            {
                string[] stringValues = model.AssetFileIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value = long.Parse(stringValue);
                    list.Add(value);
                }
            }

            return list;
        }
    }
}
