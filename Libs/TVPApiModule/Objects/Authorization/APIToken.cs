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
        [JsonIgnore]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("udid")]
        public string UDID { get; set; }

        [JsonProperty("create_date")]
        public double CreateDate { get; set; }

        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return GetAPITokenId(AccessToken); }
        }

        public APIToken(string appId, int groupId, string udid)
        {
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            CreateDate = TimeHelper.ConvertToUnixTimestamp(DateTime.UtcNow);
            AppId = appId;
            GroupID = groupId;
        }

        public static string GetAPITokenId(string accessToken)
        {
            return string.Format("access_{0}", accessToken);
        }

        
    }
}
