using System.Collections.Generic;

namespace LiveToVod.BOL
{
    public interface IRepository
    {
        LiveToVodPartnerConfiguration GetPartnerConfiguration(long partnerId);
        IEnumerable<LiveToVodLinearAssetConfiguration> GetLinearAssetConfigurations(long partnerId);
        LiveToVodLinearAssetConfiguration GetLinearAssetConfiguration(long partnerId, long linearAssetId);
        bool UpsertPartnerConfiguration(long partnerId, LiveToVodPartnerConfiguration config, long updaterId);
        bool UpsertLinearAssetConfiguration(long partnerId, LiveToVodLinearAssetConfiguration config, long updaterId);
    }
}