using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UnifiedSearchResponseWithRequestId : UnifiedSearchResponse
    {
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }
    }
}
