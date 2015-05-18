using ApiObjects.CouchbaseWrapperObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Authorization
{
    public class GroupConfiguration : CbDocumentBase
    {
        private const string GROUP_CONFIGURATION_ID_FORMAT = "group_config_{0}";

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("access_token_expiration_seconds")]
        public long AccessTokenExpirationSeconds { get; set; }

        [JsonProperty("refresh_token_expiration_seconds")]
        public long RefreshTokenExpirationSeconds { get; set; }

        [JsonProperty("is_refresh_token_extendable")]
        public bool IsRefreshTokenExtendable { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return GetGroupConfigId(GroupId); }
        }

        public static string GetGroupConfigId(int groupId)
        {
            return string.Format(GROUP_CONFIGURATION_ID_FORMAT, groupId);
        }
    }
}
