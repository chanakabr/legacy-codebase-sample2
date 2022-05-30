using WebAPI.Models.LiveToVod;

namespace WebAPI.Validation
{
    public interface ILiveToVodLinearAssetConfigurationValidator
    {
        void Validate(long partnerId, KalturaLiveToVodLinearAssetConfiguration configuration, string argumentName);
        void ValidateLinearAssetId(long partnerId, long linearAssetId, string argumentName);
    }
}