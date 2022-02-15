using System;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Upload;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class BulkUploadObjectDataMapper
    {
        private static readonly string bulkUploadMediaAssetObjectType = typeof(KalturaMediaAsset).Name;
        private static readonly string bulkUploadProgramAssetObjectType = typeof(KalturaProgramAsset).Name;
        private static readonly string bulkUploadLiveAssetObjectType = typeof(KalturaLiveAsset).Name;
        private static readonly string bulkUploadUdidDynamicListObjectType = typeof(KalturaUdidDynamicList).Name;

        public static string GetBulkUploadObjectType(this KalturaBulkUploadObjectData model)
        {
            switch (model)
            {
                case KalturaBulkUploadLiveAssetData c: return bulkUploadLiveAssetObjectType;
                case KalturaBulkUploadMediaAssetData c: return bulkUploadMediaAssetObjectType;
                case KalturaBulkUploadProgramAssetData c: return bulkUploadProgramAssetObjectType;
                case KalturaBulkUploadUdidDynamicListData c: return bulkUploadUdidDynamicListObjectType;
                default: throw new NotImplementedException($"GetBulkUploadObjectType for {model.objectType} is not implemented");
            }
        }
    }
}
