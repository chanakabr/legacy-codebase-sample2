using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common.OperationResult
{
    public class OperateResult
    {
        [JsonProperty("ok")]
        public bool Success;

        [JsonProperty("error")]
        public string ErrorMessage;

        [JsonProperty("acknowledged")]
        public bool Acknowledged;

        [JsonIgnore]
        public string JsonString { set; get; }
    }
}
