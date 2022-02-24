using System.Threading.Tasks;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using IngestHandler.Common;

namespace IngestHandler
{
    public interface IIngestFinalizer
    {
        Task FinalizeEpgIngest(BulkUploadIngestEvent serviceEvent, BulkUpload bulkUpload, BulkUploadResultsDictionary relevantResults);
    }
}