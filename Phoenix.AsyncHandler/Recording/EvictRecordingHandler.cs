using System;
using System.Text;
using System.Threading.Tasks;
using Core.Recordings;
using Grpc.Core;
using Newtonsoft.Json;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kronos;
using Phoenix.Generated.Tasks.Scheduled.EvictRecording;

namespace Phoenix.AsyncHandler.Recording
{
    public class EvictRecordingHandler : IKronosTaskHandler
    {
        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            try
            {
                EvictRecording data = JsonConvert.DeserializeObject<EvictRecording>(Encoding.UTF8.GetString(request.TaskBody.ToByteArray()));

                if (!data.PartnerId.HasValue || !data.RecordingId.HasValue || !data.OldRecordingDuration.HasValue)
                {
                    return Task.FromResult(new ExecuteTaskResponse
                    {
                        IsSuccess = false,
                        Message = "Received invalid changed evacuate recording recording data."
                    });
                }

                var status = PaddedRecordingsManager.Instance.EvacuateRecording((int) data.PartnerId.Value, data.OldRecordingDuration.Value, data.RecordingId.Value);
            
                if (!status)
                {
                    return Task.FromResult(new ExecuteTaskResponse
                    {
                        IsSuccess = false,
                        Message = "Evacuate recording recording process failed."
                    });
                }
            
                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = true,
                    Message = "Evacuate recording recording process is completed."
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Evacuate recording process failed."
                });
            }
        }
    }
}