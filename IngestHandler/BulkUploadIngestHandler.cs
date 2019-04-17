using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using Core.Catalog.CatalogManagement;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace IngestHandler
{
    public class BulkUploadIngestHandler : IServiceEventHandler<BulkUploadIngestEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public Task Handle(BulkUploadIngestEvent serviceEvent)
        {
            try
            {
                _Logger.Debug($"Starting BulkUploadIngestHandler  requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                
                ValidateServiceEvent(serviceEvent);

               
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in BulkUploadIngestHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", ex);
                return Task.FromException(ex);
            }

            return Task.CompletedTask;

        }

        private void ValidateServiceEvent(BulkUploadIngestEvent serviceEvent)
        {
            // TODO: for sunny
            // Get Ingest Profile by ingest Profile ID
            // according to defaultAutoFillPolicy if 2 and have holes reject input
            // 
            throw new NotImplementedException();
        }
    }
    
}
