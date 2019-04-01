using ApiObjects.Response;
using Newtonsoft.Json;
using System;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadJobData
    {
        public abstract GenericListResponse<GenericResponse<IBulkUploadObject>> Deserialize(long bulkUploadId, string fileUrl, BulkUploadObjectData objectData);
    }

    /// <summary>
    /// Instructions for upload data type with Excel
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadExcelJobData : BulkUploadJobData
    {
        public override GenericListResponse<GenericResponse<IBulkUploadObject>> Deserialize(long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            var response = new GenericListResponse<GenericResponse<IBulkUploadObject>>();
            var excelResults = ExcelManager.Deserialize(bulkUploadId, fileUrl, objectData);
            if (!excelResults.IsOkStatusCode())
            {
                response.SetStatus(excelResults.Status);
            }
            else
            {
                response.Objects.AddRange(excelResults.Objects);
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }
    }

    /// <summary>
    /// Instructions for ingest of custom data file
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadIngestJobData : BulkUploadJobData
    {
        public int IngestProfileId { get; set; }

        public override GenericListResponse<GenericResponse<IBulkUploadObject>> Deserialize(long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            var response = new GenericListResponse<GenericResponse<IBulkUploadObject>>();
            // TODO: Arthur Download the file
            // TODO: Arthur Call Adapater to deserialize the data inside the file
            
            return response;
        }
    }
}