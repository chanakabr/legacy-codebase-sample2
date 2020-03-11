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

        public ApiToken()
        {
        }

        //public ApiToken(string userId, int groupId, string udid, bool isAdmin, Group groupConfig, bool isLongRefreshExpiration, int regionId, Dictionary<string,string> privileges = null)
        //{
        //    string payload = KSUtils.PrepareKSPayload(new KS.KSData(udid, (int)DateUtils.GetUtcUnixTimestampNow(), regionId));
        //    this.RefreshToken = Utils.Utils.Generate32LengthGuid();
        //    this.GroupID = groupId;
        //    this.UserId = userId;
        //    this.IsAdmin = isAdmin;
        //    this.IsLongRefreshExpiration = isLongRefreshExpiration;
        //    this.Udid = udid;
        //    this.RegionId = regionId;

        //    if (isLongRefreshExpiration)
        //    {
        //        RefreshTokenExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds));
        //    }
        //    else
        //    {
        //        RefreshTokenExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
        //    }

        //    set access expiration time -no longer than refresh expiration(not relative)
        //     check if user is anonymous
        //    long accessExpiration;
        //    if (UserId == "0")
        //    {
        //        accessExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.AnonymousKSExpirationSeconds));
        //    }
        //    else
        //    {
        //        accessExpiration = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddSeconds(groupConfig.KSExpirationSeconds));
        //    }

        //    this.AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

        //    this.KsObject = new KS(isAdmin ? groupConfig.AdminSecret : groupConfig.UserSecret,
        //                      groupId.ToString(),
        //                      userId,
        //                      (int)(AccessTokenExpiration - DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow)), // relative
        //                      isAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
        //                      payload,
        //                      privileges,
        //                      Models.KS.KSVersion.V2);

        //    this.KS = KsObject.ToString();
        //}

        public ApiToken(string userId, int groupId, KS.KSData payload, bool isAdmin, Group groupConfig, bool isLongRefreshExpiration, Dictionary<string, string> privileges = null)
        {
            var ksData = new KS.KSData(payload, (int)DateUtils.GetUtcUnixTimestampNow());
            this.RefreshToken = Utils.Utils.Generate32LengthGuid();
            this.GroupID = groupId;
            this.UserId = userId;
            this.IsAdmin = isAdmin;
            this.IsLongRefreshExpiration = isLongRefreshExpiration;
            this.Udid = payload.UDID;
            this.RegionId = payload.RegionId;
            this.UserSegments = payload.UserSegments;
            this.UserRoles = payload.UserRoles;

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
                              isAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                              ksData,
                              privileges,
                              Models.KS.KSVersion.V2);

            this.KS = KsObject.ToString();
        }

        public ApiToken(ApiToken token, Group groupConfig, string udid)
        {
            var ksData = new KS.KSData(token, (int)DateUtils.GetUtcUnixTimestampNow(), udid);
            this.RefreshToken = token.RefreshToken;
            this.GroupID = token.GroupID;
            this.UserId = token.UserId;
            this.IsAdmin = token.IsAdmin;
            this.IsLongRefreshExpiration = token.IsLongRefreshExpiration;
            this.Udid = udid;
            this.RegionId = token.RegionId;
            this.UserSegments = token.UserSegments;
            this.UserRoles = token.UserRoles;

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
                              token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                              ksData,
                              null, 
                              Models.KS.KSVersion.V2);
            this.KS = KsObject.ToString();
        }
    }
}