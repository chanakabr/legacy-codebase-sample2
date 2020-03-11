using ApiObjects.CouchbaseWrapperObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Authorization
{
    public class DeviceToken : CbDocumentBase
    {
        private string _appId;

        [JsonProperty("udid")]
        public string UDID { get; set; }
        
        [JsonProperty("device_token")]
        public string Token { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return GetDeviceTokenId(_appId, Token); }    
        }

        public DeviceToken(string appId, string udid)
        {
            UDID = udid;
            Token = Guid.NewGuid().ToString().Replace("-", string.Empty);
            _appId = appId;

        }

        public static string GetDeviceTokenId(string app_id, string token)
        {
            return string.Format("device_{0}_{1}", app_id, token);
        }
    }
}
