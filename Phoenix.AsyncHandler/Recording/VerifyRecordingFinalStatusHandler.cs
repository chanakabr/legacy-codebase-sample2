using System.Text;
using System.Threading.Tasks;
using Core.Recordings;
using Grpc.Core;
using Newtonsoft.Json;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kronos;
using Phoenix.Generated.Tasks.Scheduled.VerifyRecordingFinalStatus;

namespace Phoenix.AsyncHandler.Recording
{
    public class VerifyRecordingFinalStatusHandler : IKronosTaskHandler 
    {
        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            VerifyRecordingFinalStatus data = JsonConvert.DeserializeObject<VerifyRecordingFinalStatus>(Encoding.UTF8.GetString(request.TaskBody.ToByteArray()));

            if (!data.PartnerId.HasValue || !data.RecordingId.HasValue)
            {
                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Received invalid verify recording final status data."
                });
            }
            
            var status = PaddedRecordingsManager.Instance.GetRecordingStatus((int)data.PartnerId.Value, data.RecordingId.Value);
            
            if (status == null || status.Code != 0)
            {
                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Verify recording final status process failed."
                });
            }
            
            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = true,
                Message = "Verify recording final status  is completed."
            });
        }
    }
}