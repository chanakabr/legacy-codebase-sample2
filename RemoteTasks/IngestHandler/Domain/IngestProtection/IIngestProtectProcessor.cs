using System;
using ApiObjects;
using ApiObjects.BulkUpload;

namespace IngestHandler.Domain.IngestProtection
{
    public interface IIngestProtectProcessor
    {
        void ProcessIngestProtect(CRUDOperations<EpgProgramBulkUploadObject> crudOperations, Lazy<string[]> protectedMetasAndTagsLazy);
    }
}