using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RemoteTasksService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        Task<Stream> GetMetrics();
        
        [OperationContract]
        AddTaskResponse AddTask(AddTaskRequest request);
    }
}
