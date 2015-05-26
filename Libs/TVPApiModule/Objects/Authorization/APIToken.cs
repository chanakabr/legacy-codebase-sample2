using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ApiObjects.CouchbaseWrapperObjects;
using TVPApiModule.Helper;


namespace TVPApiModule.Objects.Authorization
{
    public class APIToken : CbDocumentBase
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("site_guid")]
        public string SiteGuid { get; set; }

        [JsonProperty("access_token_expiration")]
        public long AccessTokenExpiration { get; set; }

        [JsonProperty("refresh_token_expiration")]
        public long RefreshTokenExpiration { get; set; }

        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return GetAPITokenId(AccessToken); }
        }

        public APIToken()
        {
        }

        public APIToken(string siteGuid, int groupId, bool isAdmin, GroupConfiguration groupConfig, bool isSTB)
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            AccessTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
            RefreshTokenExpiration = isSTB ? 
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds)) : 
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            GroupID = groupId;
            SiteGuid = siteGuid;
            IsAdmin = isAdmin;
        }

        public APIToken(APIToken token, long accessExpiration)
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = token.RefreshToken;
            AccessTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(accessExpiration));
            RefreshTokenExpiration = token.RefreshTokenExpiration;
            GroupID = token.GroupID;
            SiteGuid = token.SiteGuid;
            IsAdmin = token.IsAdmin;
        }

        public static string GetAPITokenId(string accessToken)
        {
            return string.Format("access_{0}", accessToken);
        }

        
    }
}
