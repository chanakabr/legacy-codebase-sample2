using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class GenericRulesResponse
    {
        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("rules")]
        public List<GenericRule> Rules { get; set; }

        public GenericRulesResponse()
        {
        }

        public GenericRulesResponse(ApiObjects.Rules.GenericRuleResponse response)
        {
            Status = new Status(response.Status.Code, response.Status.Message);

            if (response.Rules != null)
            {
                Rules = response.Rules.Select(rule => new GenericRule(rule)).ToList();
            }
            else
            {
                this.Rules = new List<GenericRule>();
            }
        }
    }
}
