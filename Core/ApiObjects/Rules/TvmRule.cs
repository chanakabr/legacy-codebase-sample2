using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    [Serializable]
    public abstract class TvmRule : Rule
    {
        [JsonProperty("CreateDate")]
        public long CreateDate { get; set; }

        [JsonProperty("RuleType")]
        public TvmRuleType RuleType { get; protected set; }
    }
}
