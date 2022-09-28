using ApiObjects.Response;
using OTT.Lib.MongoDB;

namespace IngestHandler.Common.Repositories.Models
{
    [MongoDbIgnoreExternalElements]
    public class BulkUploadErrorDocument
    {
        public long BulkUploadId { get; set; }
        public Status Error { get; set; }

        public BulkUploadErrorDocument(long bulkUploadId, Status err)
        {
            BulkUploadId = bulkUploadId;
            Error = err;
        }
    }
}