using System;
using System.Threading.Tasks;
using Core.ConditionalAccess;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Catalog;
using Phoenix.AsyncHandler.Couchbase;
using Phoenix.AsyncHandler.Kronos;
using Phoenix.Generated.Tasks.Recurring.RecordingsScheduledTasks;

namespace Phoenix.AsyncHandler.ConditionalAccess
{
    public class RecordingsScheduledTasksHandler : IKronosTaskHandler
    {
        private readonly ILogger<RecordingsScheduledTasksHandler> _logger;
        private readonly ICouchbaseWorker _couchbaseWorker;

        public RecordingsScheduledTasksHandler(ILogger<RecordingsScheduledTasksHandler> logger, ICouchbaseWorker couchbaseWorker)
        {
            _logger = logger;
            _couchbaseWorker = couchbaseWorker;
        }

        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            var featureToggel = _couchbaseWorker.GetKronosFeatureToggel();
            var kronosKey = _couchbaseWorker.GetKronosKeyName();

            if (featureToggel != null &&
                featureToggel.TryGetValue(RecordingsScheduledTasks.RecordingsScheduledTasksQualifiedName, out string toggle) &&
                toggle.Equals(kronosKey))
            {
                BaseConditionalAccess t = new TvinciConditionalAccess(0);
                try
                {
                    if (t.HandleRecordingsScheduledTasks() > 0)
                    {
                        _logger.LogDebug("Recordings scheduled tasks process is completed");
                        return Task.FromResult(new ExecuteTaskResponse
                        {
                            IsSuccess = true,
                            Message = "Recordings scheduled tasks process is completed."
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in recordings scheduled tasks: ex = {0}, ST = {1}", ex.Message,
                        ex.StackTrace);
                }

                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Error occurred in recordings scheduled tasks process."
                });
            }
            _logger.LogDebug("Recordings scheduled tasks toggle is remote task");
            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = true,
                Message = "Recordings scheduled tasks toggle is remote task."
            });
        }
    }
}