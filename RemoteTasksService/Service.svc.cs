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
using KLogMonitor;
using System.Reflection;

namespace RemoteTasksService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service : IService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "tasks")]
        public AddTaskResponse AddTask(AddTaskRequest request)
        {
            AddTaskResponse response = new AddTaskResponse();

            try
            {
                log.Debug("Info - " + string.Concat("Add Task Request Started: ", request.task));

                string taskHandlerName = TCMClient.Settings.Instance.GetValue<string>(string.Format("CELERY_ROUTING.{0}", request.task));

                log.Debug("Info - " + string.Format("Request: {0} should be handled by taskHandlerName: {1}", request.task, string.IsNullOrEmpty(taskHandlerName) ? string.Empty : taskHandlerName));

                ITaskHandler taskHandler = (ITaskHandler)Activator.CreateInstance(Type.GetType(string.Format("{0}.TaskHandler, {0}", taskHandlerName)));

                response.retval = taskHandler.HandleTask(request.data);
                response.status = "success";

                log.Debug("Info - " + string.Concat("Add Task Request Success: ", request.task));
            }
            catch (Exception ex)
            {
                log.Error("Error - Add Task Request Failed", ex);

                response.status = "failure";
                response.reason = ex.Message;
            }

            return response;
        }
    }
}
