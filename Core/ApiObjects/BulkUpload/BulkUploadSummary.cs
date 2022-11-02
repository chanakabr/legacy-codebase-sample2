using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects.BulkUpload
{
    public class BulkUploadStatistics
    {
        public long Pending { get; set; }
        public long Uploaded { get; set; }
        public long Queued { get; set; }
        public long Parsing { get; set; }
        public long Processing { get; set; }
        public long Processed { get; set; }
        public long Success { get; set; }
        public long Partial { get; set; }
        public long Failed { get; set; }
        public long Fatal { get; set; }
    }
}