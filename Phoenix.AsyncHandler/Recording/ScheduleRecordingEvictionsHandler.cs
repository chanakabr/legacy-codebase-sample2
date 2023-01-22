using System;
using System.Threading.Tasks;
using Core.Recordings;
using Grpc.Core;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Couchbase;
using Phoenix.AsyncHandler.Kronos;
using Microsoft.Extensions.Logging;

namespace Phoenix.AsyncHandler.Recording
{
   public class ScheduleRecordingEvictionsHandler: IKronosTaskHandler
    {
        private readonly ILogger _logger;
        private readonly ICouchbaseWorker _couchbaseWorker;


        public ScheduleRecordingEvictionsHandler(ILogger<ScheduleRecordingEvictionsHandler> logger)
        {
            _logger = logger;
        }

        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            try
            {
                if (PaddedRecordingsManager.Instance.ScheduleRecordingEvictions())
                {
                    _logger.LogDebug("Scheduled recording evacuation process is completed");
                    return Task.FromResult(new ExecuteTaskResponse
                    {
                        IsSuccess = true,
                        Message = "Scheduled recording evacuation process is completed."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in scheduled recording evacuation: ex = {0}, ST = {1}", ex.Message, ex.StackTrace);
            }

            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = false,
                Message = "Error occurred in  scheduled recording evacuation process."
            });
        }
    }
}