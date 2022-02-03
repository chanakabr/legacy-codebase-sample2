using System.Runtime.CompilerServices;
using KalturaRequestContext;
using Ott.Lib.FeatureToggle;
using Ott.Lib.FeatureToggle.Managers;

namespace FeatureFlag
{
    public class PhoenixFeatureFlag: IPhoenixFeatureFlag
    {
        private readonly IRequestContextUtils _requestContextUtils;
        private readonly IFeatureToggle _featureFlag;
        
        public PhoenixFeatureFlag() : this(
            new RequestContextUtils(), FeatureToggleManager.Instance())
        {
        }

        private PhoenixFeatureFlag(
            IRequestContextUtils requestContextUtils,
            IFeatureToggle featureFlag)
        {
            _requestContextUtils = requestContextUtils;
            _featureFlag = featureFlag;
        }

        public bool IsEpgNotificationEnabled(int groupId) => _featureFlag.Enabled("epg.notification", GetUser(groupId));
        public bool IsUdidDynamicListAsExcelEnabled(int groupId) => _featureFlag.Enabled("dynamicList.format", GetUser(groupId));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private KalturaFeatureToggleUser GetUser(int? groupId)
        {
            var userId = _requestContextUtils.GetUserId();
            return KalturaFeatureFlagUserBuilder.Get()
                .WithUserId(userId)
                .WithGroupId(groupId)
                .Build();
        }
    }
}