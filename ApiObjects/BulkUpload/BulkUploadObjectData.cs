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
        
        /// <summary>
        /// This creates a new bulk upload result that will display the details of a single item inside the entire bulk upload process
        /// </summary>
        /// <param name="bulkUploadId"></param>
        /// <param name="bulkUploadObject">the parsed object from the data input of bulk upload request</param>
        /// <param name="itemStatus">the item deserialization status</param>
        /// <param name="index">the index of the item in the list</param>
        /// <param name="errorStatus">in case error in deserialization this will include the error details status </param>
        /// <returns></returns>
        public abstract BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, Status errorStatusDetails);
        public abstract void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> results);

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