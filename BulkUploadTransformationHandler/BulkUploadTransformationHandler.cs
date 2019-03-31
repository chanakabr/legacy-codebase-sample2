using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ApiObjects.EventBus;
using EventBus.Abstraction;
using KLogMonitor;
using Microsoft.Extensions.Logging;
using Core.Catalog.CatalogManagement;

namespace IngestTransformationHandler
{
    public class BulkUploadTransformationHandler : IServiceEventHandler<BulkUploadTransformationEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public BulkUploadTransformationHandler()
        {
        }

        public Task Handle(BulkUploadTransformationEvent serviceEvent)
        {
            _Logger.Info($"I'm handling it, requestId:[{serviceEvent.RequestId}], jobId:[{serviceEvent.BulkUploadData.Id}]");
            // TODO: Download file from s3
            // TODO: send file data to transformation adapater
            //...

            BulkUploadManager.ProcessBulkUpload(serviceEvent.GroupId, serviceEvent.UserId, serviceEvent.BulkUploadData.Id);
            
            return Task.CompletedTask;
        }
     
    }

}