using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ApiObjects.CouchbaseWrapperObjects;


namespace TVPApiModule.Objects.Authorization
{
    public class APIToken : CbDocumentBase
    {
        private string _udid;

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("create_date")]
        public DateTime CreateDate { get; set; }

        [JsonIgnore]
        public override string Id
        {
            get { return string.Format("api_token_{0}", _udid); }
        }

        public APIToken(string udid)
        {
            _udid = udid;
            AccessToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            RefreshToken = Guid.NewGuid().ToString().Replace("-", string.Empty);
            CreateDate = DateTime.UtcNow;
        }



        internal void RefreshAccessToken()
        {
            throw new NotImplementedException();
        }
    }
}
