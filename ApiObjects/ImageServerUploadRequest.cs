using System;

namespace ApiObjects
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
