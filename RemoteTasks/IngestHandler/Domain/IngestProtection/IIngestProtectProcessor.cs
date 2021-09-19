using ApiObjects;
using ApiObjects.BulkUpload;

namespace IngestHandler.Domain.IngestProtection
{
    public interface IIngestProtectProcessor
    {
        void ProcessIngestProtect(int groupId, CRUDOperations<EpgProgramBulkUploadObject> crudOperations);
    }
}