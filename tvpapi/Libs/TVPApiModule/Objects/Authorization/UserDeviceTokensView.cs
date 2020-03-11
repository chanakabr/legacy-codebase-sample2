using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ApiObjects.CouchbaseWrapperObjects;
using TVPApiModule.Helper;


namespace TVPApiModule.Objects.Authorization
{
    public class UserDeviceTokensView : CbDocumentBase
    {
        [JsonProperty("access_token_id")]
        public string AccessTokenId { get; set; }

        [JsonProperty("refresh_token_id")]
        public string RefreshTokenId { get; set; }

        [JsonProperty("site_guid")]
        public string SiteGuid { get; set; }

        [JsonProperty("udid")]
        public string UDID { get; set; }

        [JsonProperty("access_token_expiration")]
        public long AccessTokenExpiration { get; set; }

        [JsonProperty("refresh_token_expiration")]
        public long RefreshTokenExpiration { get; set; }

        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return GetViewId(SiteGuid, UDID); }
        }

        public UserDeviceTokensView()
        {
        }

        public static string GetViewId(string siteGuid, string udid)
        {
            return string.Format("user_{0}_udid_{1}", siteGuid, udid);
        }
    }
}
