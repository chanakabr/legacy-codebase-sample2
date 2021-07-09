using System.Runtime.CompilerServices;
using KalturaRequestContext;
using LaunchDarkly.Client;

namespace FeatureFlag
{
    public class LaunchDarklyFeatureFlag : IFeatureFlag
    {
        private readonly LdClient _ldClient; // should be singleton. covered by Lazy in FeatureFlagInstance
        private readonly IRequestContextUtils _requestContextUtils;

        public LaunchDarklyFeatureFlag(string sdkKey)
        {
            _ldClient = new LdClient(sdkKey);
            _requestContextUtils = new RequestContextUtils();
        }

        public bool IsEpgNotificationEnabled(int groupId) => Enabled("epg.notification", GetUser(groupId));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Enabled(string key, User user)
        {
            var b = _ldClient.BoolVariation(key, user);
            return b;
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private User GetUser(int groupId)
        {
            var userId = _requestContextUtils.GetUserId();
            var userIdString = (userId ?? 0).ToString();
            var user = User.Builder(userIdString)
                .Anonymous(userId == null || userId == 0)
                .Custom("groupId", groupId)
                .Build();
            return user;
        }
    }
}