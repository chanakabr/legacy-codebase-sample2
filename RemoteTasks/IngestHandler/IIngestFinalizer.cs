using System.Threading.Tasks;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using IngestHandler.Common;

namespace IngestHandler
{
    public interface IIngestFinalizer
    {
        Task FinalizeEpgV2Ingest(BulkUploadIngestEvent serviceEvent, BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults);
        Task FinalizeEpgV3Ingest(int partnerId, BulkUpload bulkUpload);
    }
}