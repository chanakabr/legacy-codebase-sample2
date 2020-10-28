using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using EventBus.RabbitMQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.EventBus;
using KLogMonitor;
using System.Reflection;
using Synchronizer;
using ConfigurationManager;
using System.Runtime.Serialization;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadEpgAssetData : BulkUploadObjectData
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        // TODO Arthur - remove disterbutedTask and ruting key from media assets and use the event bus instead.
        public override string DistributedTask { get { return "disterbuted task not supported for epg ingest, use event bus instead"; } }
        public override string RoutingKey { get { return "disterbuted task not supported for epg ingest, use event bus instead"; } }

        private static readonly Type bulkUploadObjectType = typeof(EpgProgramBulkUploadObject);

        public override Type GetObjectType()
        {
            return bulkUploadObjectType;
        }
        
        public override void EnqueueObjects(BulkUpload bulkUpload, List<BulkUploadResult> objects)
        {
            throw new NotImplementedException("Enqueue logic for ingest events is happening in the Transformation Handler");
        }

        public override BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, IBulkUploadObject bulkUploadObject, int index, List<Status> errorStatusDetails)
        {
            throw new NotImplementedException();
        }

        public override IBulkUploadStructureManager GetStructureManager()
        {
            throw new NotImplementedException();
        }
    }


    [Serializable]
    public class OverlapException : Exception
    {
        public OverlapException()
        {
        }

        public OverlapException(string message) : base(message)
        {
        }

        public OverlapException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OverlapException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}