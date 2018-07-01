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
using Newtonsoft.Json;
using ConfigurationManager;

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
                string actionImplementation = string.Empty;

                if (request != null)
                {
                    // update request ID
                    string requestId = string.Empty;
                    if (ExtractRequestID(request.data, ref requestId))
                        if (!KlogMonitorHelper.MonitorLogsHelper.UpdateHeaderData(KLogMonitor.Constants.REQUEST_ID_KEY, requestId))
                            log.ErrorFormat("Error while trying to update request ID. request: {0}, req_id: {1}", JsonConvert.SerializeObject(request), requestId);

                    // extract action if exists
                    if (!string.IsNullOrEmpty(request.data))
                        ExtractActionImplementation(request.data, ref actionImplementation);
                }

                log.DebugFormat("Info - Add Task Request Started: {0}, data: {1}", request.task, request.data);

                // get task handler name (with/without action)
                string taskHandlerName = string.Empty;
                if (string.IsNullOrEmpty(actionImplementation))
                    taskHandlerName = ApplicationConfiguration.CeleryRoutingConfiguration.GetHandler(request.task);
                else
                    taskHandlerName = ApplicationConfiguration.CeleryRoutingConfiguration.GetHandler(string.Format("{0}.{1}", request.task, actionImplementation));

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

        private bool ExtractRequestID(string messageData, ref string requestId)
        {
            try
            {
                requestId = string.Empty;
                if (messageData != null)
                {
                    var reqIdContainer = JsonConvert.DeserializeObject<RequestID>(messageData);
                    if (reqIdContainer != null && !string.IsNullOrEmpty(reqIdContainer.RequestId))
                    {
                        requestId = reqIdContainer.RequestId;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error extracting request ID. messageData: {0}, ex: {1}", messageData, ex);
            }
            return false;
        }

        private bool ExtractActionImplementation(string extraParams, ref string action)
        {
            try
            {
                action = string.Empty;
                if (!string.IsNullOrEmpty(extraParams))
                {
                    var actionContainer = JsonConvert.DeserializeObject<Action>(extraParams);
                    if (actionContainer != null && !string.IsNullOrEmpty(actionContainer.ActionImp))
                    {
                        action = actionContainer.ActionImp;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error extracting request ID. messageData: {0}, ex: {1}", extraParams, ex);
            }
            return false;
        }

    }
}
