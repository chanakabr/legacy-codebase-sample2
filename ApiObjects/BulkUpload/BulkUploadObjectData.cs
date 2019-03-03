using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadObjectData
    {
        // TODO SHIR - ask TANTAN HOW TO REMOVE FROM HERE TO BulkUploadExcelJobData
        public abstract ExcelStructure GetExcelStructure(int groupId);
        public abstract IBulkUploadObject CreateObjectInstance();
    }
}