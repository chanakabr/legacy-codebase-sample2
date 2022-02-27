using System;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class EpgIngestStartedParameters
    {
        public int GroupId { get; set; }
        public long UserId { get; set; }
        public long BulkUploadId { get; set; }
        public int? IngestProfileId { get; set; }
        public string IngestFileName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}