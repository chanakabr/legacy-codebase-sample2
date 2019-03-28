using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ApiObjects.EventBus;
using EventBus.Abstraction;
using KLogMonitor;
using Microsoft.Extensions.Logging;
using Core.Catalog.CatalogManagement;

namespace EPGTransformationHandler
{
    public class SimpleMessageServiceEventEventHandler : IServiceEventHandler<EpgTransformationEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public SimpleMessageServiceEventEventHandler()
        {
        }

        public Task Handle(EpgTransformationEvent serviceEvent)
        {
            _Logger.Info($"I'm handling it, requestId:[{serviceEvent.RequestId}], jobId:[{serviceEvent.BulkUpload.Id}]");
            // TODO: Download file from s3
            // TODO: send file data to transformation adapater
            //...

            BulkUploadManager.ProcessBulkUpload(serviceEvent.GroupId, serviceEvent.UserId, serviceEvent.BulkUpload.Id);
            
            return Task.CompletedTask;
        }
     
    }

}