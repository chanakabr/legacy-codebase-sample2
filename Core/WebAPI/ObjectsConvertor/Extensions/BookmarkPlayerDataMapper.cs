using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class BookmarkPlayerDataMapper
    {
        internal static int getAverageBitRate(this KalturaBookmarkPlayerData model)
        {
            return model.averageBitRate.HasValue ? model.averageBitRate.Value : 0;
        }

        internal static int getCurrentBitRate(this KalturaBookmarkPlayerData model)
        {
            return model.currentBitRate.HasValue ? model.currentBitRate.Value : 0;
        }

        internal static int getTotalBitRate(this KalturaBookmarkPlayerData model)
        {
            return model.totalBitRate.HasValue ? model.totalBitRate.Value : 0;
        }

        internal static long getFileId(this KalturaBookmarkPlayerData model)
        {
            return model.FileId.HasValue ? model.FileId.Value : 0;
        }
    }
}
