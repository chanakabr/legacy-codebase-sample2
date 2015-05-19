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

        [JsonProperty("create_date")]
        public long CreateDate { get; set; }

        [JsonProperty("refresh_token_update_date")]
        public long RefreshTokenUpdateDate { get; set; }

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

        public APIToken(string siteGuid, int groupId, bool isAdmin) :
            this(siteGuid, groupId, isAdmin, Guid.NewGuid().ToString().Replace("-", string.Empty))
        {
        }

        public APIToken(string siteGuid, int groupId, bool isAdmin, string refreshToken)
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = refreshToken;
            CreateDate = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow);
            RefreshTokenUpdateDate = CreateDate;
            GroupID = groupId;
            SiteGuid = siteGuid;
            IsAdmin = isAdmin;
        }

        public static string GetAPITokenId(string accessToken)
        {
            return string.Format("access_{0}", accessToken);
        }

        
    }
}
