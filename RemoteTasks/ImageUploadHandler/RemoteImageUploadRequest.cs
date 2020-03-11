using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUploadHandler
{
    [Serializable]
    public class RemoteImageUploadRequest
    {
        [JsonProperty("group_id")]        
        public int GroupId { get; set; }

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
        public int MediaType { get; set; }
    }
}
