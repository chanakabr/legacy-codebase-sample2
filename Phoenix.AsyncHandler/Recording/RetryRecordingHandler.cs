using System.Text;
using System.Threading.Tasks;
using Core.Recordings;
using Grpc.Core;
using Newtonsoft.Json;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kronos;
using Phoenix.Generated.Tasks.Scheduled.RetryRecording;

namespace Phoenix.AsyncHandler.Recording
{
    public class RetryRecordingHandler : IKronosTaskHandler
    {
        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            RetryRecording data = JsonConvert.DeserializeObject<RetryRecording>(Encoding.UTF8.GetString(request.TaskBody.ToByteArray()));

            if (!data.PartnerId.HasValue || !data.RecordingId.HasValue)
            {
                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Received invalid retry recording data."
                });
            }
            
            var status = PaddedRecordingsManager.Instance.RecordRetry((int)data.PartnerId.Value, data.RecordingId.Value);
            
            if (status == null  || status.Code != 0)
            {
                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Retry recording process failed."
                });
            }
            
            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = true,
                Message = "Retry recording process is completed."
            });

        }
    }
}