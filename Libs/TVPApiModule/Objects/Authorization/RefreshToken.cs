using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ApiObjects.CouchbaseWrapperObjects;
using TVPApiModule.Helper;


namespace TVPApiModule.Objects.Authorization
{
    public class RefreshToken : CbDocumentBase
    {
        [JsonProperty("access_token_id")]
        public string AccessTokenId { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshTokenValue { get; set; }

        [JsonProperty("site_guid")]
        public string SiteGuid { get; set; }

        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonProperty("refresh_token_expiration")]
        public long RefreshTokenExpiration { get; set; }

        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("is_long_refresh_expiration")]
        public bool IsLongRefreshExpiration { get; set; }

        [JsonProperty("udid")]
        public string UDID{ get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return GetRefreshTokenId(RefreshTokenValue); }
        }

        public RefreshToken()
        {
        }

        public RefreshToken(APIToken token)
        {
            AccessTokenId = token.Id;
            RefreshTokenValue = token.RefreshToken;
            GroupID = token.GroupID;
            SiteGuid = token.SiteGuid;
            RefreshTokenExpiration = token.RefreshTokenExpiration;
            IsAdmin = token.IsAdmin;
            IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            UDID = token.UDID;
        }

        public RefreshToken(string siteGuid, int groupId, string accessTokenId, string refreshToken, long refreshTokenExpiration, bool isAdmin, bool isLongRefreshExpiration, string udid)
        {
            AccessTokenId = accessTokenId;
            RefreshTokenValue = refreshToken;
            GroupID = groupId;
            SiteGuid = siteGuid;
            RefreshTokenExpiration = refreshTokenExpiration;
            IsAdmin = isAdmin;
            IsLongRefreshExpiration = isLongRefreshExpiration;
            UDID = udid;
        }

        public static string GetRefreshTokenId(string refreshToken)
        {
            return string.Format("refresh_{0}", refreshToken);
        }
    }
}
