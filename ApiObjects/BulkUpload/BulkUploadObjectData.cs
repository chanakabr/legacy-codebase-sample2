using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadObjectData
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        public abstract string DistributedTask { get; }
        public abstract string RoutingKey { get; }

        public abstract IBulkUploadStructure GetStructure();
        public abstract IBulkUploadObject CreateObjectInstance();
        public abstract Dictionary<string, object> GetMandatoryPropertyToValueMap();
        public abstract BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, BulkUploadResultStatus status, int index, Status errorStatus);
        public abstract void EnqueueObjects(BulkUpload bulkUpload, List<Tuple<Status, IBulkUploadObject>> objects);

        public bool Validate(Dictionary<string, object> propertyToValueMap)
        {
            var mandatoryPropertyToValueMap = GetMandatoryPropertyToValueMap();
            foreach (var mandatoryPropertyToValue in mandatoryPropertyToValueMap)
            {
                if (!propertyToValueMap.ContainsKey(mandatoryPropertyToValue.Key) ||
                    !propertyToValueMap[mandatoryPropertyToValue.Key].Equals(mandatoryPropertyToValue.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}