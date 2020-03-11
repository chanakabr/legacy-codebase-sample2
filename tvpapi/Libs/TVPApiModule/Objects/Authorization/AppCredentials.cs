using ApiObjects.CouchbaseWrapperObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Manager;

namespace TVPApiModule.Objects.Authorization
{
    public class AppCredentials : CbDocumentBase
    {
        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("app_id")]
        public string EncryptedAppId { get; set; }

        [JsonProperty("app_secret")]
        public string EncryptedAppSecret { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return GetAppCredentialsId(EncryptedAppId); }
        }

        //public AppCredentials(int groupId)
        //{
        //    EncryptedAppId = AuthorizationManager.Instance.EncryptData(Guid.NewGuid().ToString().Replace("-", string.Empty));
        //    EncryptedAppSecret = AuthorizationManager.Instance.EncryptData(Guid.NewGuid().ToString().Replace("-", string.Empty));
        //    GroupId = groupId;
        //}

        public static string GetAppCredentialsId(string appId)
        {
            return string.Format("app_{0}", appId);
        }
    }
}
