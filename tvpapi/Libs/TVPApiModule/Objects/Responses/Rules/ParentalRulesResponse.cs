using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.Objects.Responses
{
    public class ParentalRulesResponse
    {
        [JsonProperty()]
        public Status status;

        [JsonProperty()]
        public List<ParentalRule> rules;

        public ParentalRulesResponse()
        {
            this.status = new Status();
            this.rules = new List<ParentalRule>();
        }

        /// <summary>
        /// Create a response based on a response from WS_API
        /// </summary>
        /// <param name="copy"></param>
        public ParentalRulesResponse(ApiObjects.ParentalRulesResponse copy)
        {
            this.status = new Status(copy.status.Code, copy.status.Message);

            if (copy.rules != null)
            {
                this.rules = copy.rules.Select(rule => new ParentalRule(rule)).ToList();
            }
            else
            {
                this.rules = new List<ParentalRule>();
            }
        }
    }
}
