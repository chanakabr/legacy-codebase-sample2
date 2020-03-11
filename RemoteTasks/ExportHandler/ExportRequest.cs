using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExportHandler
{
    [Serializable]
    public class ExportRequest
    {
        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("task_id")]
        public long TaskId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
