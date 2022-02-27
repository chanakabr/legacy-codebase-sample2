using System;
using System.Collections.Generic;
using ApiObjects.BulkUpload;
using ApiObjects.Response;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class EpgIngestCompletedParameters
    {
        public int GroupId { get; set; }
        public long UserId { get; set; }
        public long BulkUploadId { get; set; }
        public IEnumerable<BulkUploadResult> Results { get; set; }
        public BulkUploadJobStatus Status { get; set; }
        public IEnumerable<Status> Errors { get; set; }
        public DateTime CompletedDate { get; set; }
    }
}