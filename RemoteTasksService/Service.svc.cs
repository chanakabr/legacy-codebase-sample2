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
                Logger.Logger.Log("Info", string.Concat("Add Task Request Started: ", request.task), "RemoteTasksService");

                string taskHandlerName = TCMClient.Settings.Instance.GetValue<string>(string.Format("CELERY_ROUTING.{0}", request.task));

                ITaskHandler taskHandler = (ITaskHandler)Activator.CreateInstance(Type.GetType(string.Format("{0}.TaskHandler, {0}", taskHandlerName)));

                response.retval = taskHandler.HandleTask(request.data);
                response.status = "success";

                Logger.Logger.Log("Info", string.Concat("Add Task Request Success: ", request.task), "RemoteTasksService");
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Add Task Request Fail: {0}, Error: {1}, Stacktrace: {2}", request.task, ex.Message, ex.StackTrace), "RemoteTasksService");

                response.status = "failure";
                response.reason = ex.Message;
            }

            return response;
        }
    }
}
