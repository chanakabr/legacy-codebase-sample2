using System.Collections.Generic;
using Newtonsoft.Json;

namespace SessionManager
{
    public class UserSessions
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_revocation")]
        public long UserRevocation { get; set; }

        [JsonProperty("user_with_udid_revocations")]
        public Dictionary<string, long> UserWithUdidRevocations { get; set; }

        [JsonProperty("expiration")]
        public long expiration { get; set; }

        public UserSessions()
        {
            UserWithUdidRevocations = new Dictionary<string, long>();
        }
    }
}