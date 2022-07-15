using System.Threading.Tasks;
using Grpc.Core;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;

namespace Phoenix.AsyncHandler.Kronos
{
    public interface IKronosTaskHandler
    {
        Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context);
    }
}