using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ApiObjects.CouchbaseWrapperObjects;
using TVPApiModule.Helper;
using TVPApi;
using TVPApiModule.Manager;
using KSWrapper;
using ConfigurationManager;

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
        public string UDID{ get; private set; }

        [JsonProperty("platform")]
        public PlatformType Platform { get; set; }
        
        [JsonIgnore]
        public override string Id
        {
            get { return GetAPITokenId(AccessToken); }
        }

        [JsonIgnore]
        public KS KsObject { get; set; }

        [JsonProperty("regionId")]
        public int RegionId { get; set; }

        [JsonProperty("UserSegments")]
        public List<long> UserSegments { get; set; }

        [JsonProperty("UserRoles")]
        public List<long> UserRoles { get; set; }

        [JsonProperty("Signature")]
        public string Signature { get; set; }

        public APIToken()
        {
        }

        public APIToken(string userId, int groupId, KSData ksData, bool isAdmin, GroupConfiguration groupConfig, bool isLongRefreshExpiration, Group group, Dictionary<string, string> privileges = null)
        {
            var payload = new KSData(ksData, (int)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow));
            RefreshToken = Generate32LengthGuid();
            GroupID = groupId;
            SiteGuid = userId;
            IsAdmin = isAdmin;
            IsLongRefreshExpiration = isLongRefreshExpiration;
            
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
                              (int)(isAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER),
                              payload,
                              privileges,
                              KSVersion.V2,
                              ApplicationConfiguration.Current.RequestParserConfiguration.KsSecrets);
            payload = KsObject.ExtractKSData();
            SetKsData(payload);

            AccessToken = KsObject.ToString();
        }

        public APIToken(string userId, int groupId, KSData ksData, bool isAdmin, GroupConfiguration groupConfig, bool isLongRefreshExpiration, PlatformType platform)
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
            SiteGuid = userId;
            IsAdmin = isAdmin;
            IsLongRefreshExpiration = isLongRefreshExpiration;
            Platform = platform;
            SetKsData(ksData);
        }

        public APIToken(APIToken token, Group group, GroupConfiguration groupConfig, string udid)
        {
            var payload = new KSData(udid, (int)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow), token.RegionId, token.UserSegments, token.UserRoles, token.Signature);
            RefreshToken = token.RefreshToken;
            GroupID = token.GroupID;
            SiteGuid = token.SiteGuid;
            IsAdmin = token.IsAdmin;
            IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            
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
                              (int)(token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER),
                              payload,
                              null,
                              KSVersion.V2,
                              ApplicationConfiguration.Current.RequestParserConfiguration.KsSecrets);

            payload = KsObject.ExtractKSData();
            SetKsData(payload);
            AccessToken = KsObject.ToString();
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
            RegionId = token.RegionId;
            UserSegments = token.UserSegments;
            UserRoles = token.UserRoles;
            Signature = token.Signature;
        }
        
        public static string GetAPITokenId(string accessToken)
        {
            return string.Format("access_{0}", accessToken);
        }

        public KS CreateKS(string tokenVal)
        {
            var sessionType = (int)(this.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER);
            var data = new KSData(this.UDID, 0, this.RegionId, this.UserSegments, this.UserRoles, this.Signature).PrepareKSPayload();
            var ks = new KS(this.GroupID, this.SiteGuid, this.AccessTokenExpiration, sessionType, data, tokenVal);
            return ks;
        }

        private static string Generate32LengthGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        private void SetKsData(KSData ksData)
        {
            UDID = ksData.UDID;
            RegionId = ksData.RegionId;
            UserSegments = ksData.UserSegments;
            UserRoles = ksData.UserRoles;
            Signature = ksData.Signature;
        }
    }
}
