using EventBus.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class RemoteImageUploadRequest : ServiceEvent
    {
        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("version")]        
        public int Version { get; set; }

        [JsonProperty("source_path")]        
        public string SourcePath { get; set; }

        [JsonProperty("row_id")]
        public long RowId { get; set; }

        [JsonProperty("image_server_url")]
        public string ImageServerUrl { get; set; }

        [JsonProperty("media_type")]
        public ApiObjects.eMediaType MediaType { get; set; }
    }
}
