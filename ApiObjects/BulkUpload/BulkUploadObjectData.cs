using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadObjectData
    {
        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        public abstract IBulkUploadStructure GetStructure();
        public abstract IBulkUploadObject CreateObjectInstance();
        public abstract bool Validate(Dictionary<string, object> propertyToValueMap);
        public abstract Dictionary<string, object> GetMandatoryPropertyToValueMap();
    }
}