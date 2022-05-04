namespace IngestHandler.Common.Locking
{
    public static class LockInitiator
    {
        public static string GetBulkUploadLockInitiator(long bulkUploadId) => $"BulkUpload_{bulkUploadId}";

        public const string EpgIngestGlobalLockKeyInitiator = "_EPG_V2_Ingest";
    }
}
