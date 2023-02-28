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
        
        public bool IsMediaMarksNewModel(int groupId) => _featureFlag.Enabled("mediamarks-play-location-in-user-object", GetUser(groupId)); // BEO-11088
        //public bool IsUdidDynamicListAsExcelEnabled(int groupId) => _featureFlag.Enabled("dynamicList.format", GetUser(groupId));
        public bool IsStrictUnlockDisabled() => _featureFlag.Enabled("distributedlock.strict-unlock-disabled", GetUser());
        public bool IsEfficientSerializationUsed() =>_featureFlag.Enabled("is-efficient-serialization-used", GetUser());
        public bool IsRenewUseKronosPog() => _featureFlag.Enabled("is-renew-use-kronos-pog", GetUser(null));
        public bool IsRenewUseKronos() => _featureFlag.Enabled("is-renew-use-kronos", GetUser(null));
        public bool IsUnifiedRenewUseKronos() => _featureFlag.Enabled("is-unified-renew-use-kronos", GetUser());
        public bool IsRenewalReminderUseKronos() => _featureFlag.Enabled("is-renew-reminder-use-kronos", GetUser());
        public bool IsRenewSubscriptionEndsUseKronos() => _featureFlag.Enabled("is-renew-subscription-ends-use-kronos", GetUser());
        public bool IsCloudfrontInvalidationEnabled() => _featureFlag.Enabled("cloudfront-invalidation", GetUser()); //BEO-12440
        public bool IsImprovedUpdateMediaAssetStoredProcedureShouldBeUsed() => _featureFlag.Enabled("is-improved-update-media-asset-stored-procedure-should-be-used", GetUser());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private KalturaFeatureToggleUser GetUser(long? groupId = null)
        {
            var resolvedGroupId = groupId ?? _featureFlagContext.GetPartnerId();
            var userId = _featureFlagContext.GetUserId();
            return KalturaFeatureFlagUserBuilder.Get()
                .WithUserId(userId)
                .WithGroupId((int?)resolvedGroupId)
                .Build();
        }
    }
}
