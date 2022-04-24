using System.Collections.Generic;
using ApiObjects.Catalog;
using ApiObjects.Response;

namespace DAL
{
    public interface IAssetStructRepository
    {
        GenericResponse<AssetStruct> InsertAssetStruct(
            int groupId,
            long userId,
            AssetStruct assetStructToAdd,
            List<KeyValuePair<string, string>> namesInOtherLanguages,
            List<KeyValuePair<long, int>> metaIdsToPriority);

        GenericResponse<AssetStruct> UpdateAssetStruct(
            int groupId,
            long userId,
            long assetStructId,
            AssetStruct assetStructToUpdate,
            bool shouldUpdateOtherNames,
            List<KeyValuePair<string, string>> namesInOtherLanguages,
            bool shouldUpdateMetaIds,
            List<KeyValuePair<long, int>> metaIdsToPriority);

        List<AssetStruct> GetAssetStructsByGroupId(int groupId);
    }
}