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


    // TODO: Arthur\Shir move thsi wirth Excel manager and the EPPLUS nuget from ApiObjects to ApiLogic .. Objects should be clear of any logic.
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
}