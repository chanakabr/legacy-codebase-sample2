using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUploadHandler
{
    [Serializable]
    public class ImageUploadRequest
    {
        [JsonProperty("group_id")]        
        public int GroupId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("version")]        
        public int Version { get; set; }

        [JsonProperty("source_path")]        
        public string SourcePath { get; set; }

    }
}
