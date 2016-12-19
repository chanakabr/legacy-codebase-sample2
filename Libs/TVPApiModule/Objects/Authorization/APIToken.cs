using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ApiObjects.CouchbaseWrapperObjects;
using TVPApiModule.Helper;
using TVPApi;
using TVPApiModule.Manager;


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

        [JsonProperty("is_long_refresh_expiration")]
        public bool IsLongRefreshExpiration { get; set; }

        [JsonProperty("udid")]
        public string UDID{ get; set; }

        [JsonProperty("platform")]
        public PlatformType Platform { get; set; }
        
        [JsonIgnore]
        public override string Id
        {
            get { return GetAPITokenId(AccessToken); }
        }

        public APIToken()
        {
        }

        public APIToken(string siteGuid, int groupId, bool isAdmin, GroupConfiguration groupConfig, bool isLongRefreshExpiration, string udid, PlatformType platform)
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            AccessTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
            RefreshTokenExpiration = isLongRefreshExpiration ? 
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds)) : 
                AuthorizationManager.GetPlatformExpiration(groupConfig, platform, DateTime.UtcNow);
            GroupID = groupId;
            SiteGuid = siteGuid;
            IsAdmin = isAdmin;
            IsLongRefreshExpiration = isLongRefreshExpiration;
            UDID = udid;
            Platform = platform;
        }

        public APIToken(APIToken token, GroupConfiguration groupConfig)
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = token.RefreshToken;
            RefreshTokenExpiration = groupConfig.IsRefreshTokenExtendable ? 
                (token.IsLongRefreshExpiration ? token.RefreshTokenExpiration + groupConfig.RefreshExpirationForPinLoginSeconds : token.RefreshTokenExpiration + AuthorizationManager.GetPlatformExpiration(groupConfig, token.Platform)) :
                token.RefreshTokenExpiration;

            // set access expiration time - no longer than refresh expiration
            long accessExpiration =(long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
            AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;
            
            GroupID = token.GroupID;
            SiteGuid = token.SiteGuid;
            IsAdmin = token.IsAdmin;
            IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            UDID = token.UDID;
            Platform = token.Platform;
        }

        public APIToken(RefreshToken token, GroupConfiguration groupConfig)
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = token.RefreshTokenValue;
            if (groupConfig.IsRefreshTokenExtendable)
            {
                RefreshTokenExpiration = token.IsLongRefreshExpiration ? (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds)) :
                     AuthorizationManager.GetPlatformExpiration(groupConfig, token.Platform, DateTime.UtcNow);
            }
            else
            {
                RefreshTokenExpiration = token.RefreshTokenExpiration;
            }
            // set access expiration time - no longer than refresh expiration
            long accessExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
            AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

            GroupID = token.GroupID;
            SiteGuid = token.SiteGuid;
            IsAdmin = token.IsAdmin;
            IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            UDID = token.UDID;
            Platform = token.Platform;
        }

        public static string GetAPITokenId(string accessToken)
        {
            return string.Format("access_{0}", accessToken);
        }

        
    }
}
