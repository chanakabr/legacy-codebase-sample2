using ApiObjects.Response;
using Newtonsoft.Json;
using System;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadJobData
    {
        public abstract GenericListResponse<BulkUploadResult> Deserialize(long bulkUploadId, string fileUrl, BulkUploadObjectData objectData);
    }


    // TODO: Arthur\Shir move thsi wirth Excel manager and the EPPLUS nuget from ApiObjects to ApiLogic .. Objects should be clear of any logic.
    /// <summary>
    /// Instructions for upload data type with Excel
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadExcelJobData : BulkUploadJobData
    {
        public override GenericListResponse<BulkUploadResult> Deserialize(long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            var response = new GenericListResponse<BulkUploadResult>();
            var excelObjects = ExcelManager.Deserialize(bulkUploadId, fileUrl, objectData);
            if (!excelObjects.IsOkStatusCode())
            {
                response.SetStatus(excelObjects.Status);
            }
            else
            {
                for (int i = 0; i < excelObjects.Objects.Count; i++)
                {
                    var bulkUploadObject = excelObjects.Objects[i];
                    var errorStatus = bulkUploadObject.IsOkStatusCode() ? null : bulkUploadObject.Status;
                    var bulkUploadResult = objectData.GetNewBulkUploadResult(bulkUploadId, bulkUploadObject.Object, i, errorStatus);
                    response.Objects.Add(bulkUploadResult);
                }

                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }
    }
}