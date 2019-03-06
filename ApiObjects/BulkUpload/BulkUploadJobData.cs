using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadJobData
    {
        public abstract GenericListResponse<Tuple<Status, IBulkUploadObject>> Deserialize(int groupId, string fileUrl, BulkUploadObjectData objectData);
    }

    /// <summary>
    /// instractions for upload data type with Excel
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadExcelJobData : BulkUploadJobData
    {
        public override GenericListResponse<Tuple<Status, IBulkUploadObject>> Deserialize(int groupId, string fileUrl, BulkUploadObjectData objectData)
        {
            var response = new GenericListResponse<Tuple<Status, IBulkUploadObject>>();
            var excelResults = ExcelManager.Deserialize(groupId, fileUrl, objectData);
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