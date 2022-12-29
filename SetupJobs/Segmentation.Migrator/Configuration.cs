using ApiObjects;
using Core.Api;
using System.Collections.Generic;

namespace Segmentation.Migrator
{
    public class Configuration
    {
        public int PartnerId { get; set; }

        public int SegmentationTypePageSize { get; set; }

        public int UserSegmentPageSize { get; set; }
        public string CouchbaseUserName { get; set; } = "Administrator";
    }
}