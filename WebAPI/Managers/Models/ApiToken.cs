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

        public ApiToken(string siteGuid, int groupId, string udid, bool isAdmin, Group groupConfig, bool isLongRefreshExpiration)
        {
            string payload = PrepareUdidPayload(udid);
            KS ks = new KS(groupConfig.UserSecret, groupId.ToString(), siteGuid, (int)groupConfig.KSExpirationSeconds, isAdmin ? KalturaSessionType.ADMIN : KalturaSessionType.USER, payload, string.Empty);
            KS = ks.ToString();
            RefreshToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            AccessTokenExpiration = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.KSExpirationSeconds));
            RefreshTokenExpiration = isLongRefreshExpiration ?
                Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshExpirationForPinLoginSeconds)) :
                Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.RefreshTokenExpirationSeconds));
            GroupID = groupId;
            UserId = siteGuid;
            IsAdmin = isAdmin;
            IsLongRefreshExpiration = isLongRefreshExpiration;
        }

        private static string PrepareUdidPayload(string udid)
        {
            var l = new List<KeyValuePair<string, string>>();
            l.Add(new KeyValuePair<string, string>(WebAPI.Managers.Models.KS.PAYLOAD_UDID, udid));
            string payload = WebAPI.Managers.Models.KS.preparePayloadData(l);
            return payload;
        }

        public ApiToken(ApiToken token, Group groupConfig, string udid)
        {
            string payload = PrepareUdidPayload(udid);
            KS ks = new KS(groupConfig.UserSecret, token.GroupID.ToString(), token.UserId, (int)groupConfig.KSExpirationSeconds, KalturaSessionType.USER, null, string.Empty);
            KS = ks.ToString();
            RefreshToken = token.RefreshToken;
            RefreshTokenExpiration = groupConfig.IsRefreshTokenExtendable ? 
                (token.IsLongRefreshExpiration ? token.RefreshTokenExpiration + groupConfig.RefreshExpirationForPinLoginSeconds : token.RefreshTokenExpiration + groupConfig.RefreshTokenExpirationSeconds) :
                token.RefreshTokenExpiration;

            // set access expiration time - no longer than refresh expiration
            long accessExpiration = Utils.SerializationUtils.ConvertToUnixTimestamp(DateTime.UtcNow.AddSeconds(groupConfig.KSExpirationSeconds));
            AccessTokenExpiration = accessExpiration >= RefreshTokenExpiration ? RefreshTokenExpiration : accessExpiration;
            
            GroupID = token.GroupID;
            UserId = token.UserId;
            IsAdmin = token.IsAdmin;
            IsLongRefreshExpiration = token.IsLongRefreshExpiration;
        }
    }
}