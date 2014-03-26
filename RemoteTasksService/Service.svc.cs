using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Configuration;
using RemoteTasksCommon;

namespace RemoteTasksService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service : IService
    {
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "tasks")]
        public AddTaskResponse AddTask(AddTaskRequest request)
        {
            AddTaskResponse response = new AddTaskResponse();

            try
            {
                ITaskHandler taskHandler = (ITaskHandler)Activator.CreateInstance(Type.GetType(string.Format("{0}.TaskHandler, {0}", ConfigurationManager.AppSettings[request.task])));

                response.retval = taskHandler.HandleTask(request.data);
                response.status = "success";
            }
            catch (Exception ex)
            {
                response.status = "failure";
                response.reason = ex.Message;
            }

            return response;
        }
    }
}
