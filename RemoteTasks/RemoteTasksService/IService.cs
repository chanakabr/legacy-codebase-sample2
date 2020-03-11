using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using RemoteTasksCommon;

namespace RemoteTasksService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AddTaskResponse AddTask(AddTaskRequest request);
    }

    
}
