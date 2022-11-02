using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Ott.Lib.FeatureToggle;
using Ott.Lib.FeatureToggle.Managers;

namespace FeatureFlag
{
    public class PhoenixFeatureFlag: IPhoenixFeatureFlag
    {
        private readonly IFeatureFlagContext _featureFlagContext;
        private readonly IFeatureToggle _featureFlag;

        internal PhoenixFeatureFlag(IFeatureFlagContext featureFlagContext) : this(featureFlagContext, FeatureToggleManager.Instance()) { }

        [ActivatorUtilitiesConstructor]
        public PhoenixFeatureFlag(IFeatureFlagContext featureFlagContext, IFeatureToggle featureFlag)
        {
            _featureFlagContext = featureFlagContext;
            _featureFlag = featureFlag;
        }

        public bool IsEpgNotificationEnabled(int groupId) => _featureFlag.Enabled("epg.notification", GetUser(groupId));
        public bool IsMediaMarksNewModel(int groupId) => _featureFlag.Enabled("mediamarks-play-location-in-user-object", GetUser(groupId)); // BEO-11088
        
        //public bool IsUdidDynamicListAsExcelEnabled(int groupId) => _featureFlag.Enabled("dynamicList.format", GetUser(groupId));

        public bool IsStrictUnlockDisabled() => _featureFlag.Enabled("distributedlock.strict-unlock-disabled", GetUser((int?) _featureFlagContext.GetPartnerId()));
        public bool IsEfficientSerializationUsed() =>_featureFlag.Enabled("is-efficient-serialization-used", GetUser((int?) _featureFlagContext.GetPartnerId()));
        public bool IsRenewUseKronos() => _featureFlag.Enabled("is-renew-use-kronos", GetUser((int?) _featureFlagContext.GetPartnerId()));
        public bool IsUnifiedRenewUseKronos() => _featureFlag.Enabled("is-unified-renew-use-kronos", GetUser((int?) _featureFlagContext.GetPartnerId()));
        public bool IsRenewalReminderUseKronos() => _featureFlag.Enabled("is-renew-reminder-use-kronos", GetUser((int?) _featureFlagContext.GetPartnerId()));
        public bool IsRenewSubscriptionEndsUseKronos() => _featureFlag.Enabled("is-renew-subscription-ends-use-kronos", GetUser((int?) _featureFlagContext.GetPartnerId()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private KalturaFeatureToggleUser GetUser(int? groupId)
        {
            var resolvedGroupId = (int?) (groupId ?? _featureFlagContext.GetPartnerId());
            var userId = _featureFlagContext.GetUserId();
            return KalturaFeatureFlagUserBuilder.Get()
                .WithUserId(userId)
                .WithGroupId(resolvedGroupId)
                .Build();
        }
    }
}
