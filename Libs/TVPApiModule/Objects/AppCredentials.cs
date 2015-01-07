using ApiObjects.CouchbaseWrapperObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class AppCredentials : CbDocumentBase
    {
        private int _groupId;

        [JsonProperty("app_id")]
        public string AppID { get; set; }

        [JsonProperty("app_secret")]
        public string AppSecret { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return string.Format("app_{0}", _groupId); }
        }

        public AppCredentials(int groupId)
        {
            _groupId = groupId;

            AppID = Guid.NewGuid().ToString().Replace("-", string.Empty);
            AppSecret = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
