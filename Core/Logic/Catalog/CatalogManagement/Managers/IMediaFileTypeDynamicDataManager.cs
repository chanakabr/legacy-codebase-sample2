using System.Collections.Generic;
using ApiObjects.MediaFiles;
using ApiObjects.Response;

namespace ApiLogic.Catalog.CatalogManagement.Managers
{
    public interface IMediaFileTypeDynamicDataManager
    {
        /// <summary>
        /// Add new Dynamic Data value for MediaFileType
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="dynamicDataKeyValue">Dynamic Data value to add</param>
        /// <param name="userId">Id of the user called add</param>
        /// <returns>Created Dynamic Data value</returns>
        GenericResponse<MediaFileTypeDynamicDataKeyValue> AddMediaFileDynamicDataValue(int groupId,
            MediaFileTypeDynamicDataKeyValue dynamicDataKeyValue, long userId);

        /// <summary>
        /// Get list of the Dynamic Data values matched to the search criteria
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="idIn">List of Ids to fetch</param>
        /// <param name="mediaFileTypeId">MediaFileType Id</param>
        /// <param name="mediaFileTypeKeyName">Dynamic Data key name</param>
        /// <param name="valueEqual">Key value to match</param>
        /// <param name="valueStartsWith">Key value start with</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of the Dynamic Data values</returns>
        GenericListResponse<MediaFileTypeDynamicDataKeyValue> GetMediaFileDynamicDataValues(int groupId, List<long> idIn,
            long? mediaFileTypeId, string mediaFileTypeKeyName, string valueEqual, string valueStartsWith,
            int pageIndex, int pageSize);

        /// <summary>
        /// Deletes Dynamic Data value from MediaFileType and related MediaFiles.
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="id">Dynamic Data Value Id</param>
        /// <param name="userId">Id of the user called deletion</param>
        /// <returns>True if success, error status otherwise</returns>
        GenericResponse<bool> DeleteMediaFileDynamicDataValue(int groupId, long id, long userId);
    }
}
