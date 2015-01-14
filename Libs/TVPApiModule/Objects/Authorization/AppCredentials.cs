using ApiObjects.CouchbaseWrapperObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Authorization
{
    public class AppCredentials : CbDocumentBase
    {
        [JsonIgnore]
        public string AppId { get; set; }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("app_secret")]
        public string AppSecret { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return GetAppCredentialsId(AppId); }
        }

        public AppCredentials(int groupId)
        {
            AppId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            AppSecret = Guid.NewGuid().ToString().Replace("-", string.Empty);
            GroupId = groupId;
        }

        public static string GetAppCredentialsId(string appId)
        {
            return string.Format("app_{0}", appId);
        }
    }
}
