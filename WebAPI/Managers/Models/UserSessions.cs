using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Managers.Models
{
    public class UserSessions
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_revocation")]
        public int UserRevocation { get; set; }

        [JsonProperty("user_with_udid_revocations")]
        public Dictionary<string, int> UserWithUdidRevocations { get; set; }

        [JsonProperty("expiration")]
        public int expiration { get; set; }

        public UserSessions()
        {
            UserWithUdidRevocations = new Dictionary<string, int>();
        }
    }
}