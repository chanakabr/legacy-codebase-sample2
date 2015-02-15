using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DomainLimitationModuleResponse
    {
        [JsonProperty(PropertyName = "domain_limitation_module")]
        public LimitationsManager DLM { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public DomainLimitationModuleResponse()
        {
        }
    }
}
