using ApiObjects.Response;
using LiveToVod.BOL;

namespace LiveToVod
{
    public interface ILiveToVodManager
    {
        LiveToVodFullConfiguration GetFullConfiguration(long partnerId);
        LiveToVodPartnerConfiguration GetPartnerConfiguration(long partnerId);
        LiveToVodLinearAssetConfiguration GetLinearAssetConfiguration(long partnerId, long linearAssetId);
        GenericResponse<LiveToVodPartnerConfiguration> UpdatePartnerConfiguration(long partnerId, LiveToVodPartnerConfiguration config, long updaterId);
        LiveToVodLinearAssetConfiguration UpdateLinearAssetConfiguration(long partnerId, LiveToVodLinearAssetConfiguration config, long updaterId);
        LiveToVodFullConfiguration GetCachedFullConfiguration(long partnerId);
    }
}