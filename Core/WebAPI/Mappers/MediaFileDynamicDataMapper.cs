using System.Collections.Generic;
using System.Linq;
using ApiObjects.MediaFiles;
using WebAPI.Models.Catalog;

namespace WebAPI.Mappers
{
    public static class MediaFileDynamicDataMapper
    {
        public static KalturaMediaFileDynamicData Map(MediaFileTypeDynamicDataKeyValue source)
        {
            return new KalturaMediaFileDynamicData
            {
                Id = source.Id,
                MediaFileTypeId = source.MediaFileTypeId,
                MediaFileTypeKeyName = source.MediaFileTypeKeyName,
                Value = source.Value
            };
        }

        public static MediaFileTypeDynamicDataKeyValue Map(KalturaMediaFileDynamicData source)
        {
            return new MediaFileTypeDynamicDataKeyValue
            {
                Id = source.Id,
                MediaFileTypeId = source.MediaFileTypeId,
                MediaFileTypeKeyName = source.MediaFileTypeKeyName,
                Value = source.Value
            };
        }

        public static List<KalturaMediaFileDynamicData> Map(IEnumerable<MediaFileTypeDynamicDataKeyValue> source)
        {
            return source?.Select(Map).ToList();
        }
    }
}
