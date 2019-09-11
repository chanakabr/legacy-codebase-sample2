using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Abstraction;
using Newtonsoft.Json;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class ExportRequest : ServiceEvent
    {
        [JsonProperty("task_id")]
        public long TaskId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
