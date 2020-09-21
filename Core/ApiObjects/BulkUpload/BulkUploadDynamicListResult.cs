using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadDynamicListResult : BulkUploadResult
    {
    }
}
