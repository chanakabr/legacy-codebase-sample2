using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    [Serializable]
    public class Rule
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        [JsonProperty("Status")]
        public RuleStatus Status { get; set; }

        [JsonProperty("Label")]
        public string Label { get; set; }
    }
}
