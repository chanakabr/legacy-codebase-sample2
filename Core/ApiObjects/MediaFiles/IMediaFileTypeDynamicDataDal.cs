using System.Collections.Generic;
using System.Data;
using ApiObjects.Response;

namespace ApiObjects.MediaFiles
{
    public interface IMediaFileTypeDynamicDataDal
    {
        List<MediaFileTypeDynamicDataKeyValue> GetMediaFileDynamicDataKeyValuesByMediaFileTypeId(int groupId, long? mediaFileTypeId,
            string mediaFileTypeKeyName);

        List<MediaFileTypeDynamicDataKeyValue> GetMediaFileDynamicDataKeyValuesByIds(int groupId, List<long> ids);

        GenericResponse<MediaFileTypeDynamicDataKeyValue> InsertMediaFileDynamicDataValue(int groupId, long mediaFileTypeId, string keyName, string value,
            long userId);

        GenericResponse<bool> DeleteMediaFileDynamicDataValue(int groupId, long id, long userId);
    }
}
