using System;
using System.Collections.Generic;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Newtonsoft.Json;

namespace Core.Catalog
{
    /// <summary>
    /// Instructions for ingest of custom data file
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadIngestJobData : BulkUploadJobData
    {
        public int? IngestProfileId { get; set; }

        public bool DisableEpgNotification { get; set; }

        // LockKeys and DatesOfProgramsToIngest should be updated simultaneously,
        // because currently we lock by a day => LockKeys just a string-array representation of DatesOfProgramsToIngest
        public string[] LockKeys;
        public DateTime[] DatesOfProgramsToIngest;

        public override GenericListResponse<BulkUploadResult> Deserialize(int groupId, long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            throw new NotImplementedException("Ingest bulk upload deserialization is handled in TransformationHandler and this method should not be called");
        }
    }
}