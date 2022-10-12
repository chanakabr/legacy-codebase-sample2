using System.Collections.Generic;
using System.Data;

namespace Core.Catalog.CatalogManagement
{
    public interface IMediaAssetService
    {
        MediaAsset CreateMediaAsset(
            long groupId,
            DataTable basicTable,
            DataTable metasTable,
            DataTable tagsTable,
            DataTable newTagsTable,
            DataTable filesTable,
            DataTable labelsTable,
            DataTable imagesTable,
            DataTable updateDateTable,
            DataTable linearAssetTable,
            DataTable nameRelatedEntitiesTable,
            DataTable liveToVodAssetTable,
            bool isForIndex = false,
            bool isForMigration = false,
            bool isMinimalOutput = false);

        IEnumerable<MediaAsset> CreateMediaAssets(long groupId, DataSet dataSet);

        LiveAsset CreateLinearMediaAsset(long groupId, MediaAsset mediaAsset, DataTable dataTable, long? epgChannelId = null);
    }
}