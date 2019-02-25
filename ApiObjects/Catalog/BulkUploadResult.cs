using ApiObjects.Response;
using Newtonsoft.Json;
using System;

namespace ApiObjects.Catalog
{
    public abstract class BulkUploadResult
    {
        // can be assetId, userId etc
        public long ObjectId { get; set; }
        public int Index { get; set; }
        public long BulkUploadId { get; set; }
        public Status Status { get; set; }

        public BulkUploadResult()
        {
            Index = -1;
        }
        
        public override string ToString()
        {
            // TODO SHIR - BulkUploadResult ToString
            return base.ToString();
        }
    }

    public abstract class BulkUploadAssetResult : BulkUploadResult
    {
        public string ExternalId { get; set; }
    }

    public class BulkUploadMediaAssetResult : BulkUploadAssetResult
    {
        public int? Type { get; set; }
    }
}
