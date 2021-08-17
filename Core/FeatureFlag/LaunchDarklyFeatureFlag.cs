using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using LaunchDarkly.Client;

namespace FeatureFlag
{
    internal class LaunchDarklyFeatureFlag : IFeatureFlag
    {
        private const string LD_SDK_KEY_ENV = "LD_SDK_KEY";
        private const string ONE_BOX_ID_ENV = "ONE_BOX_ID";

        private readonly ILdClient _ldClient; // should be singleton. covered by Lazy in FeatureFlagInstance

        private static readonly string OneBoxIdVal = GetOneBoxId();
        
        public LaunchDarklyFeatureFlag()
        {
            _ldClient = new LdClient(GetSdkKey());
        }

        public LaunchDarklyFeatureFlag(ILdClient ldClient)
        {
            _ldClient = ldClient;
        }

        private string GetSdkKey() =>
            Environment.GetEnvironmentVariable(LD_SDK_KEY_ENV)
            ?? throw new ArgumentNullException($"Launch Darkly sdk key can't be found in {LD_SDK_KEY_ENV}");

        private static string GetOneBoxId() => Environment.GetEnvironmentVariable(ONE_BOX_ID_ENV);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Enabled(string key, KalturaFeatureFlagUser kalturaUser)
        {
            var b = _ldClient.BoolVariation(key, GetUser(kalturaUser));
            return b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private User GetUser(KalturaFeatureFlagUser kalturaUser)
        {
            var userId = kalturaUser.UserId ?? 0;
            var userIdString = userId.ToString(CultureInfo.InvariantCulture);
            var builder = User.Builder(userIdString)
                .Anonymous(kalturaUser.IsAnonymous);

            if (kalturaUser.GroupId != null && kalturaUser.GroupId != 0)
            {
                builder.Custom("groupId", kalturaUser.GroupId.Value);
            }

            if (!string.IsNullOrEmpty(OneBoxIdVal))
            {
                // env is not used for staging, pre-prod, prod
                // it is only used for creation of onebox specific rules in LD 
                builder.Custom("env", OneBoxIdVal);
            }

            return builder.Build();
        }
    }
}