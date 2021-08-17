using System.Runtime.CompilerServices;
using FeatureFlag;
using KalturaRequestContext;

namespace ApiLogic.FeatureToggle
{
    public class PhoenixFeatureFlag: IPhoenixFeatureFlag
    {
        private readonly IRequestContextUtils _requestContextUtils;
        private readonly IFeatureFlag _featureFlag;
        
        public PhoenixFeatureFlag() : this(
            new RequestContextUtils(), FeatureFlagInstance.Get())
        {
        }

        private PhoenixFeatureFlag(
            IRequestContextUtils requestContextUtils,
            IFeatureFlag featureFlag)
        {
            _requestContextUtils = requestContextUtils;
            _featureFlag = featureFlag;
        }

        public bool IsEpgNotificationEnabled(int groupId) => _featureFlag.Enabled("epg.notification", GetUser(groupId));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private KalturaFeatureFlagUser GetUser(int? groupId)
        {
            var userId = _requestContextUtils.GetUserId();
            
            return KalturaFeatureFlagUserBuilder.Get()
                .WithUserId(userId)
                .WithGroupId(groupId)
                .Build();
        }
    }
}