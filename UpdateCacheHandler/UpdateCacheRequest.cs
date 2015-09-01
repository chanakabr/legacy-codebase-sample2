using CouchbaseManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateCacheHandler
{
    [Serializable]
    public class UpdateCacheRequest
    {
        [JsonProperty("couchbase_bucket", Required=Required.AllowNull)]
        public eCouchbaseBucket Bucket
        {
            get;
            set;
        }

        [JsonProperty("keys")]
        public List<string> Keys
        {
            get;
            set;
        }
    }
}
