using System.Collections.Generic;
using ApiObjects.BulkUpload;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class EpgIngestPartCompletedParameters
    {
        public int GroupId { get; set; }
        public long UserId { get; set; }
        public long BulkUploadId { get; set; }
        public bool HasMoreEpgToIngest { get; set; }
        public IEnumerable<BulkUploadProgramAssetResult> Results { get; set; }
    }
}