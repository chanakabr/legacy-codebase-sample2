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
        private int _groupId;

        [JsonProperty("device_token")]
        public string Token { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return string.Format("device_{0}_{1}", _groupId, _udid); }    
        }

        public DeviceToken(int groupId, string udid)
        {
            _udid = udid;
            _groupId = groupId;
            Token = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
