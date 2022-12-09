using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TVinciShared;
using WebAPI.Models.General;

namespace WebAPI.Managers.Models
{
    public class ApiToken
    {
        [JsonProperty("access_token")]
        public string KS { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("site_guid")]
        public string UserId { get; set; }

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
        public string Udid { get; set; }
        
        [JsonProperty("regionId")]
        public int RegionId { get; set; }

        [JsonProperty("UserSegments")]
        public List<long> UserSegments { get; set; }

        [JsonProperty("UserRoles")]
        public List<long> UserRoles { get; set; }
        
        [JsonProperty("SessionCharacteristicKey")]
        public string SessionCharacteristicKey { get; set; }
        
        [JsonProperty("DomainId")]
        public int DomainId { get; set; }

        [JsonProperty("IsBypassCacheEligible")]
        public bool IsBypassCacheEligible { get; set; }
        
        [JsonIgnore]
        public KS KsObject { get; set; }
        
        private ApiToken(){} // is used for json deserialization

        public ApiToken(string userId, int groupId, KS.KSData payload, bool isAdmin, Group groupConfig, bool isLongRefreshExpiration, Dictionary<string, string> privileges = null)
        {
            var ksData = new KS.KSData(payload, (int)DateUtils.GetUtcUnixTimestampNow());
            RefreshToken = Utils.Utils.Generate32LengthGuid();
            GroupID = groupId;
            UserId = userId;
            IsAdmin = isAdmin;
            IsLongRefreshExpiration = isLongRefreshExpiration;
            SetPayload(payload);

            if (isLongRefreshExpiration)
            {
                RefreshTokenExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds));
            }
            else
            {
                RefreshTokenExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            }

            // set access expiration time - no longer than refresh expiration (not relative)
            // check if user is anonymous
            long accessExpiration;
            if (UserId == "0")
            {
                accessExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.AnonymousKSExpirationSeconds));
            }
            else
            {
                accessExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.KSExpirationSeconds));
            }

            AccessTokenExpiration = accessExpiration;

            if (groupConfig.IsRefreshTokenEnabled)
            {
                AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration
                    ? RefreshTokenExpiration
                    : accessExpiration;
            }

            KsObject = new KS(isAdmin ? groupConfig.AdminSecret : groupConfig.UserSecret,
                              groupId.ToString(),
                              userId,
                              (int)(AccessTokenExpiration - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)), // relative
                              isAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                              ksData,
                              privileges,
                              Models.KS.KSVersion.V2);

            KS = KsObject.ToString();
        }

        public ApiToken(ApiToken token, Group groupConfig, string udid)
        {
            var ksData = new KS.KSData(token, (int)DateUtils.GetUtcUnixTimestampNow(), udid);
            RefreshToken = token.RefreshToken;
            GroupID = token.GroupID;
            UserId = token.UserId;
            IsAdmin = token.IsAdmin;
            IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            Udid = udid;
            RegionId = token.RegionId;
            UserSegments = token.UserSegments;
            UserRoles = token.UserRoles;
            SessionCharacteristicKey = token.SessionCharacteristicKey;
            IsBypassCacheEligible = token.IsBypassCacheEligible;

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
            if (UserId == "0")
            {
                accessExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.AnonymousKSExpirationSeconds));
            }
            else
            {
                accessExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.KSExpirationSeconds));
            }

            AccessTokenExpiration = accessExpiration;

            if (groupConfig.IsRefreshTokenEnabled)
            {
                AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration
                    ? RefreshTokenExpiration
                    : accessExpiration;
            }

            KsObject = new KS(token.IsAdmin ? groupConfig.AdminSecret : groupConfig.UserSecret,
                              token.GroupID.ToString(),
                              token.UserId,
                              (int)(AccessTokenExpiration - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)),
                              token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                              ksData,
                              null, 
                              Models.KS.KSVersion.V2);
            KS = KsObject.ToString();
        }

        public ApiToken(KS ks, KS.KSData payload)
        {
            GroupID = ks.GroupId;
            AccessTokenExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(ks.Expiration);
            KS = ks.ToString();
            UserId = ks.UserId;
            SetPayload(payload);
        }
        
        private void SetPayload(KS.KSData payload)
        {
            Udid = payload.UDID;
            RegionId = payload.RegionId;
            UserSegments = payload.UserSegments;
            UserRoles = payload.UserRoles;
            SessionCharacteristicKey = payload.SessionCharacteristicKey;
            DomainId = payload.DomainId;
            IsBypassCacheEligible = payload.IsBypassCacheEligible;
        }
    }
}