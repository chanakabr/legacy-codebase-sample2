using System;
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

        public Task Handle(BulkUploadTransformationEvent serviceEvent)
        {
            try
            {
                _Logger.Debug($"Starting ingest transformation handler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                var processBulkUploadStatus = BulkUploadManager.ProcessBulkUpload(serviceEvent.GroupId, serviceEvent.UserId, serviceEvent.BulkUploadId);
                if (processBulkUploadStatus?.IsOkStatusCode() == true)
                {
                    _Logger.Debug($"ProcessBulkUpload completed successfully BulkUpload.Id:[{serviceEvent.BulkUploadId}]");
                    return Task.CompletedTask;
                }

                var ex = new Exception(string.Format("BulkUpload task did not finish successfully. {0}", processBulkUploadStatus != null ? processBulkUploadStatus.ToString() : string.Empty));
                return Task.FromException(ex);
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in BulkUploadTransformationHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", ex);
                return Task.FromException(ex);
            }
        }
    }
}