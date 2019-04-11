using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadEpgAssetData : BulkUploadObjectData
    {
        // TODO: Arthur, remove disterbutedTask and ruting key from media assets and use the event bus instead.
        public override string DistributedTask { get { return "disterbuted task not supported for epg ingest, use event bus instead"; } }
        public override string RoutingKey { get { return "disterbuted task not supported for epg ingest, use event bus instead"; } }

        public override IBulkUploadObject CreateObjectInstance()
        {
            throw new NotImplementedException();
        }
        
        public override void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> objects)
        {
            // TODO: implement split into 24Hr and send over event bus for ingest
        }

        public override Dictionary<string, object> GetMandatoryPropertyToValueMap()
        {
            throw new NotImplementedException();
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, Status errorStatus)
        {
            throw new NotImplementedException();
        }

        public override IBulkUploadStructure GetStructure()
        {
            throw new NotImplementedException();
        }
    }
}