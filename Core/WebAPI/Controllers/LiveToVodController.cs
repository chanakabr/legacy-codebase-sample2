using LiveToVod;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Mappers;
using WebAPI.Models.LiveToVod;
using WebAPI.Validation;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Live to VOD endpoints.
    /// </summary>
    [Service("liveToVod")]
    public class LiveToVodController : IKalturaController
    {
        private static readonly ILiveToVodPartnerConfigurationValidator _partnerConfigurationValidator = LiveToVodPartnerConfigurationValidator.Instance;
        private static readonly ILiveToVodLinearAssetConfigurationValidator _liveToVodLinearAssetConfigurationValidator = LiveToVodLinearAssetConfigurationValidator.Instance;
        private static readonly ILiveToVodManager _liveToVodManager = LiveToVodManager.Instance;

        /// <summary>
        /// Get existing L2V configuration for both the partner level and all channels level.
        /// </summary>
        /// <returns>Live to VOD configuration.</returns>
        [Action("getConfiguration")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        public static KalturaLiveToVodFullConfiguration GetConfiguration()
        {
            var groupId = KS.GetFromRequest().GroupId;

            var config = _liveToVodManager.GetFullConfiguration(groupId);
            var result = LiveToVodMapper.Map(config);

            return result;
        }

        /// <summary>
        /// Get existing L2V partner configuration.
        /// </summary>
        /// <returns>Live to VOD partner configuration.</returns>
        [Action("getPartnerConfiguration")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        public static KalturaLiveToVodPartnerConfiguration GetPartnerConfiguration()
        {
            var groupId = KS.GetFromRequest().GroupId;

            var config = _liveToVodManager.GetPartnerConfiguration(groupId);
            var result = LiveToVodMapper.Map(config);

            return result;
        }

        /// <summary>
        /// Get existing L2V configuration for a specific linear asset.
        /// </summary>
        /// <param name="linearAssetId">Linear asset's identifier.</param>
        /// <returns>Returns a joined account level and linear asset level values.
        /// 1. isL2vEnabled: If account level parameter is false, then return false. Otherwise return linear channel level value.
        /// 2. retentionPeriodDays: If null then return account level parameter, otherwise return linear channel level value.</returns>
        [Action("getLinearAssetConfiguration")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        public static KalturaLiveToVodLinearAssetConfiguration GetLinearAssetConfiguration(long linearAssetId)
        {
            var groupId = KS.GetFromRequest().GroupId;
            _liveToVodLinearAssetConfigurationValidator.ValidateLinearAssetId(groupId, linearAssetId, nameof(linearAssetId));

            var config = _liveToVodManager.GetLinearAssetConfiguration(groupId, linearAssetId);
            var result = LiveToVodMapper.Map(config);

            return result;
        }

        /// <summary>
        /// Set L2V configuration on the partner level.
        /// </summary>
        /// <param name="configuration">Live to VOD configuration object.</param>
        /// <returns>Updated Live to VOD partner configuration.</returns>
        [Action("updatePartnerConfiguration")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        public static KalturaLiveToVodPartnerConfiguration UpdatePartnerConfiguration(KalturaLiveToVodPartnerConfiguration configuration)
        {
            _partnerConfigurationValidator.Validate(configuration, nameof(configuration));

            var groupId = KS.GetFromRequest().GroupId;
            var userId = Utils.Utils.GetUserIdFromKs();

            var mappedConfig = LiveToVodMapper.Map(configuration);
            var updatedConfig = _liveToVodManager.UpdatePartnerConfiguration(groupId, mappedConfig, userId);
            var result = LiveToVodMapper.Map(updatedConfig);

            return result;
        }

        /// <summary>
        /// Set L2V configuration for a specific Linear channel.
        /// </summary>
        /// <param name="configuration">Live to VOD linear asset (live channel) configuration object.</param>
        /// <returns>Updated Live to VOD linear asset (live channel) configuration.</returns>
        [Action("updateLinearAssetConfiguration")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        public static KalturaLiveToVodLinearAssetConfiguration UpdateLinearAssetConfiguration(KalturaLiveToVodLinearAssetConfiguration configuration)
        {
            var groupId = KS.GetFromRequest().GroupId;
            _liveToVodLinearAssetConfigurationValidator.Validate(groupId, configuration, nameof(configuration));

            var userId = Utils.Utils.GetUserIdFromKs();

            var mappedConfig = LiveToVodMapper.Map(configuration);
            var updatedConfig = _liveToVodManager.UpdateLinearAssetConfiguration(groupId, mappedConfig, userId);
            var result = LiveToVodMapper.Map(updatedConfig);

            return result;
        }
    }
}