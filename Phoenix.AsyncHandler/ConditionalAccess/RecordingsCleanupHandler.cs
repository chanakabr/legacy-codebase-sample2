using System;
using System.Threading.Tasks;
using Core.ConditionalAccess;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Couchbase;
using Phoenix.AsyncHandler.Kronos;
using Phoenix.Generated.Tasks.Recurring.RecordingsCleanup;

namespace Phoenix.AsyncHandler.ConditionalAccess
{
    public class RecordingsCleanupHandler : IKronosTaskHandler
    {
        private readonly ILogger<RecordingsCleanupHandler> _logger;
        private readonly ICouchbaseWorker _couchbaseWorker;

        public RecordingsCleanupHandler(ILogger<RecordingsCleanupHandler> logger, ICouchbaseWorker couchbaseWorker)
        {
            _logger = logger;
            _couchbaseWorker = couchbaseWorker;
        }

        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            var featureToggel = _couchbaseWorker.GetKronosFeatureToggel();
            var kronosKey = _couchbaseWorker.GetKronosKeyName();
            if (featureToggel != null &&
                featureToggel.TryGetValue(RecordingsCleanup.RecordingsCleanupQualifiedName,  out string toggle) &&
                toggle.Equals(kronosKey))
            {
                BaseConditionalAccess t = new TvinciConditionalAccess(0);
                try
                {
                    t.CleanupRecordings();
                    _logger.LogDebug("Cleanup recordings process is completed");
                    return Task.FromResult(new ExecuteTaskResponse
                    {
                        IsSuccess = true,
                        Message = "Cleanup recordings process is completed."
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in cleanup recordings: ex = {0}, ST = {1}", ex.Message, ex.StackTrace);
                    return Task.FromResult(new ExecuteTaskResponse
                    {
                        IsSuccess = false,
                        Message = "Error occurred in cleanup recordings process."
                    });
                }
            }
            _logger.LogDebug("Cleanup recordings toggle is remote task");
            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = true,
                Message = "Cleanup recordings toggle is remote task."
            });
        }
    }
}