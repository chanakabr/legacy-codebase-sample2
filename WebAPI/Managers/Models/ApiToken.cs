using ConfigurationManager;
using KSWrapper;
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
        
        [JsonIgnore]
        public KS KsObject { get; set; }

        // TODO SHIR - SET Signature IN ALL LIKE Udid
        [JsonProperty("Signature")]
        public string Signature { get; set; }

        public ApiToken()
        {
        }

        public ApiToken(string userId, int groupId, KSData payload, bool isAdmin, Group groupConfig, bool isLongRefreshExpiration, Dictionary<string, string> privileges = null)
        {
            var ksData = new KSData(payload, (int)DateUtils.GetUtcUnixTimestampNow());
            this.RefreshToken = Utils.Utils.Generate32LengthGuid();
            this.GroupID = groupId;
            this.UserId = userId;
            this.IsAdmin = isAdmin;
            this.IsLongRefreshExpiration = isLongRefreshExpiration;

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

            this.AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

            this.KsObject = new KS(isAdmin ? groupConfig.AdminSecret : groupConfig.UserSecret,
                              groupId.ToString(),
                              userId,
                              (int)(AccessTokenExpiration - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)), // relative
                              (int)(isAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER),
                              ksData,
                              privileges,
                              KSVersion.V2,
                              ApplicationConfiguration.Current.RequestParserConfiguration.KsSecrets);
            ksData = this.KsObject.ExtractKSData();
            SetKsData(ksData);

            this.KS = KsObject.ToString();
        }

        public ApiToken(ApiToken token, Group groupConfig, string udid)
        {
            var ksData = new KSData(udid, (int)DateUtils.GetUtcUnixTimestampNow(), token.RegionId, token.UserSegments, token.UserRoles, token.Signature);
            this.RefreshToken = token.RefreshToken;
            this.GroupID = token.GroupID;
            this.UserId = token.UserId;
            this.IsAdmin = token.IsAdmin;
            this.IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            
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

            this.AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

            this.KsObject = new KS(token.IsAdmin ? groupConfig.AdminSecret : groupConfig.UserSecret,
                              token.GroupID.ToString(),
                              token.UserId,
                              (int)(AccessTokenExpiration - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)),
                              (int)(token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER),
                              ksData,
                              null, 
                              KSVersion.V2,
                              ApplicationConfiguration.Current.RequestParserConfiguration.KsSecrets);
            ksData = this.KsObject.ExtractKSData();
            SetKsData(ksData);

            this.KS = KsObject.ToString();
        }

        public KS CreateKS(string tokenVal)
        {
            var sessionType = (int)(this.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER);
            var data = new KSData(this.Udid, 0, this.RegionId, this.UserSegments, this.UserRoles).PrepareKSPayload();
            var ks = new KS(this.GroupID, this.UserId, this.AccessTokenExpiration, sessionType, data, tokenVal);
            return ks;
        }

        private void SetKsData(KSData ksData)
        {
            this.Udid = ksData.UDID;
            this.RegionId = ksData.RegionId;
            this.UserSegments = ksData.UserSegments;
            this.UserRoles = ksData.UserRoles;
            this.Signature = ksData.Signature;
        }
    }
}