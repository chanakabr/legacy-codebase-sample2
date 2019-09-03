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

        [JsonIgnore]
        public KS KsObject { get; set; }

        public APIToken()
        {
        }

        public APIToken(string siteGuid, int groupId, bool isAdmin, GroupConfiguration groupConfig, bool isLongRefreshExpiration, string udid, PlatformType platform)
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            AccessTokenExpiration = isLongRefreshExpiration ? 
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessExpirationForPinLoginSeconds)) : 
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
            RefreshTokenExpiration = isLongRefreshExpiration ?
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds)) :
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
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
                (token.IsLongRefreshExpiration ? token.RefreshTokenExpiration + groupConfig.RefreshExpirationForPinLoginSeconds : 
                token.RefreshTokenExpiration + groupConfig.RefreshTokenExpirationSeconds) :
                token.RefreshTokenExpiration;

            // set access expiration time - no longer than refresh expiration
            long accessExpiration = token.IsLongRefreshExpiration ? 
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessExpirationForPinLoginSeconds)) :
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
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
                RefreshTokenExpiration = token.IsLongRefreshExpiration ? 
                    (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds)) :
                    (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            }
            else
            {
                RefreshTokenExpiration = token.RefreshTokenExpiration;
            }
            // set access expiration time - no longer than refresh expiration
            long accessExpiration = token.IsLongRefreshExpiration ?
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessExpirationForPinLoginSeconds)) :
                (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
            AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

            GroupID = token.GroupID;
            SiteGuid = token.SiteGuid;
            IsAdmin = token.IsAdmin;
            IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            UDID = token.UDID;
            Platform = token.Platform;
        }

        public APIToken(string userId, int groupId, string udid, bool isAdmin, Group group, GroupConfiguration groupConfig, bool isLongRefreshExpiration, Dictionary<string, string> privileges = null)
        {
            string payload = KSUtils.PrepareKSPayload(new KS.KSData() { UDID = udid, CreateDate = (int)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow) });
            RefreshToken = Generate32LengthGuid();
            GroupID = groupId;
            SiteGuid = userId;
            IsAdmin = isAdmin;
            IsLongRefreshExpiration = isLongRefreshExpiration;
            UDID = udid;
            if (isLongRefreshExpiration)
            {
                RefreshTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds));
            }
            else
            {
                RefreshTokenExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            }

            // set access expiration time - no longer than refresh expiration (not relative)
            // check if user is anonymous
            long accessExpiration;
            if (SiteGuid == "0")
            {
                accessExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds)); // in phoenix - group.AnonymousKSExpirationSeconds
            }
            else
            {
                accessExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
            }

            AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

            KsObject = new KS(isAdmin ? group.AdminSecret : group.UserSecret,
                groupId.ToString(),
                userId,
                (int)(AccessTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)), // relative
                isAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                payload,
                privileges,
                KS.KSVersion.V2);

            AccessToken = KsObject.ToString();
        }

        public APIToken(APIToken token, Group group, GroupConfiguration groupConfig, string udid)
        {
            string payload = KSUtils.PrepareKSPayload(new KS.KSData() { UDID = udid, CreateDate = (int)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow) });
            RefreshToken = token.RefreshToken;
            GroupID = token.GroupID;
            SiteGuid = token.SiteGuid;
            IsAdmin = token.IsAdmin;
            IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            UDID = udid;
            // set refresh token expiration
            if (groupConfig.IsRefreshTokenExtendable)
            {
                RefreshTokenExpiration = token.IsLongRefreshExpiration ?
                    token.RefreshTokenExpiration + groupConfig.RefreshExpirationForPinLoginSeconds :
                    token.RefreshTokenExpiration + groupConfig.RefreshTokenExpirationSeconds;
            }
            else
            {
                RefreshTokenExpiration = token.RefreshTokenExpiration;
            }

            // set access expiration time - no longer than refresh expiration
            // check if user is anonymous
            long accessExpiration;
            if (SiteGuid == "0")
            {
                accessExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds)); /// in Phoenix - group.AnonymousKSExpirationSeconds
            }
            else
            {
                accessExpiration = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AccessTokenExpirationSeconds));
            }

            AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

            KsObject = new KS(token.IsAdmin ? group.AdminSecret : group.UserSecret,
                token.GroupID.ToString(),
                token.SiteGuid,
                (int)(AccessTokenExpiration - TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow)),
                token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                payload,
                null,
                KS.KSVersion.V2);
            AccessToken = KsObject.ToString();
        }


        public static string GetAPITokenId(string accessToken)
        {
            return string.Format("access_{0}", accessToken);
        }

        private static string Generate32LengthGuid()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
