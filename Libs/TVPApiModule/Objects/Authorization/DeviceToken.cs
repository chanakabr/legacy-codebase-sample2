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
        private string _udid;

        [JsonProperty("device_token")]
        public string Token { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return string.Format("device_token_{0}", _udid); }    
        }

        public DeviceToken(string udid)
        {
            _udid = udid;
            Token = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
