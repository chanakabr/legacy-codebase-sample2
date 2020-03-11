using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUploadHandler
{
    [Serializable]
    public class ImageServerUploadRequest
    {
        public int GroupId { get; set; }

        public string Id { get; set; }

        public int Version { get; set; }

        public string SourcePath { get; set; }
    }
}
