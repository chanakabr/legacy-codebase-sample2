using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.DRM
{
    [DataContract]
    public class CencResponse
    {
        [JsonProperty("key_id")]
        public string KeyId { get; set; }

        [JsonProperty("content_id")]
        public string ContentId { get; set; }

        [JsonProperty("next_key_id")]
        public string NextKeyId { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("pssh")]
        public List<Pssh> pssh { get; set; }
    }

    public class Pssh
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }
}
