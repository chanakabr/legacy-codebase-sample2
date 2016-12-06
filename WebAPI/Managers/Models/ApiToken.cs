using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        public ApiToken()
        {
        }

        public ApiToken(string userId, int groupId, string udid, bool isAdmin, Group groupConfig, bool isLongRefreshExpiration)
        {
            string payload = KSUtils.PrepareKSPayload(new WebAPI.Managers.Models.KS.KSData() { UDID = udid });
            RefreshToken = Utils.Utils.Generate32LengthGuid();
            GroupID = groupId;
            UserId = userId;
            IsAdmin = isAdmin;
            IsLongRefreshExpiration = isLongRefreshExpiration;
            Udid = udid;
            if (isLongRefreshExpiration)
            {
                RefreshTokenExpiration = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds));
            }
            else
            {
                RefreshTokenExpiration = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            }

            // set access expiration time - no longer than refresh expiration (not relative)
            // check if user is anonymous
            long accessExpiration;
            if (UserId == "0")
            {
                accessExpiration = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AnonymousKSExpirationSeconds));
            }
            else
            {
                accessExpiration = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.KSExpirationSeconds));
            }

            AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

            KS ks = new KS(isAdmin ? groupConfig.AdminSecret : groupConfig.UserSecret,
                groupId.ToString(),
                userId,
                (int)(AccessTokenExpiration - Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)), // relative
                isAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                payload,
                new List<KalturaKeyValue>(), 
                Models.KS.KSVersion.V2);

            KS = ks.ToString();
        }

        public ApiToken(ApiToken token, Group groupConfig, string udid)
        {
            string payload = KSUtils.PrepareKSPayload(new WebAPI.Managers.Models.KS.KSData() { UDID = udid });
            RefreshToken = token.RefreshToken;
            GroupID = token.GroupID;
            UserId = token.UserId;
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
            if (UserId == "0")
            {
                accessExpiration = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.AnonymousKSExpirationSeconds));
            }
            else
            {
                accessExpiration = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.KSExpirationSeconds));
            }

            AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;

            KS ks = new KS(token.IsAdmin ? groupConfig.AdminSecret : groupConfig.UserSecret,
                token.GroupID.ToString(),
                token.UserId,
                (int)(AccessTokenExpiration - Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow)),
                token.IsAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER,
                payload,
                new List<KalturaKeyValue>(), 
                Models.KS.KSVersion.V2);
            KS = ks.ToString();
        }
    }
}