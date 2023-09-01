using System;
using System.Threading.Tasks;
using Core.ConditionalAccess;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Catalog;
using Phoenix.AsyncHandler.Couchbase;
using Phoenix.AsyncHandler.Kronos;
using Phoenix.Generated.Tasks.Recurring.RecordingsLifetime;

namespace Phoenix.AsyncHandler.ConditionalAccess
{
    public class RecordingsLifetimeHandler: IKronosTaskHandler
    {
        private readonly ILogger<RecordingsLifetimeHandler> _logger;
        private readonly ICouchbaseWorker _couchbaseWorker;


        public RecordingsLifetimeHandler(ILogger<RecordingsLifetimeHandler> logger, ICouchbaseWorker couchbaseWorker)
        {
            _logger = logger;
            _couchbaseWorker = couchbaseWorker;
        }

        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            var featureToggel = _couchbaseWorker.GetKronosFeatureToggel();
            var kronosKey = _couchbaseWorker.GetKronosKeyName();

            if (featureToggel != null &&
                featureToggel.TryGetValue(RecordingsLifetime.RecordingsLifetimeQualifiedName,  out string toggle) &&
                toggle.Equals(kronosKey))
            {
                BaseConditionalAccess t = new TvinciConditionalAccess(0);
                try
                {
                    if (t.HandleRecordingsLifetime() > -1)
                    {
                        _logger.LogDebug("Recordings lifetime process is completed");
                        return Task.FromResult(new ExecuteTaskResponse
                        {
                            IsSuccess = true,
                            Message = "Recordings lifetime process is completed."
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in recordings lifetime: ex = {0}, ST = {1}", ex.Message, ex.StackTrace);
                }

                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Error occurred in recordings lifetime process."
                });
            }
            _logger.LogDebug("Recordings lifetime toggle is remote task");
            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = true,
                Message = "Recordings lifetime toggle is remote task."
            });
        }
    }
}