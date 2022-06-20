using DAL.DTO;

namespace DAL
{
    public interface ILiveToVodAssetRepository
    {
        bool TryGetMediaIdByEpgId(long epgId, out long mediaId);

        long? InsertLiveToVodAsset(LiveToVodAssetDTO liveToVodAssetDto);

        bool UpdateLiveToVodAsset(LiveToVodAssetDTO liveToVodAssetDto);
    }
}