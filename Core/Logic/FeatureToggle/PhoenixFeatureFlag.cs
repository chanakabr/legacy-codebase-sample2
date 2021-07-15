using System.Runtime.CompilerServices;
using FeatureFlag;
using KalturaRequestContext;

namespace ApiLogic.FeatureToggle
{
    public class PhoenixFeatureFlag: IPhoenixFeatureFlag
    {
        private readonly IRequestContextUtils _requestContextUtils;
        private readonly IFeatureFlag _featureFlag;
        private readonly IKalturaFeatureFlagUserBuilder _kalturaFeatureFlagUserBuilder;

        /// <summary>
        /// Constructor with dummy feature flag instantiation
        /// </summary>
        public PhoenixFeatureFlag() : this(
            new RequestContextUtils(),new LaunchDarklyDummyFeatureFlag(), new KalturaFeatureFlagUserBuilder())
        {
        }

        private PhoenixFeatureFlag(
            IRequestContextUtils requestContextUtils,
            IFeatureFlag featureFlag,
            IKalturaFeatureFlagUserBuilder kalturaFeatureFlagUserBuilder)
        {
            _requestContextUtils = requestContextUtils;
            _featureFlag = featureFlag;
            _kalturaFeatureFlagUserBuilder = kalturaFeatureFlagUserBuilder;
        }

        public bool IsEpgNotificationEnabled(int groupId) => _featureFlag.Enabled("epg.notification", GetUser(groupId));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private KalturaFeatureFlagUser GetUser(int groupId)
        {
            var userId = _requestContextUtils.GetUserId();
            
            return _kalturaFeatureFlagUserBuilder
                .WithUserId(userId)
                .WithGroupId(groupId)
                .Build();
        }
    }
}