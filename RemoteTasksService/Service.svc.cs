using System;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
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
                var actionImplementation = string.Empty;
                var requestId = string.Empty;
                if (request != null)
                {
                    // update request ID
                    if (ExtractRequestID(request.data, ref requestId) && !KlogMonitorHelper.MonitorLogsHelper.UpdateHeaderData(Constants.REQUEST_ID_KEY, requestId))
                    {
                        log.Error($"Error while trying to update request ID. request: {JsonConvert.SerializeObject(request)}, req_id: {requestId}.");
                    }
                        
                    // extract action if exists
                    if (!string.IsNullOrEmpty(request.data))
                    {
                        ExtractActionImplementation(request.data, ref actionImplementation);
                    }
                }

                log.DebugFormat("Info - Add Task Request Started: {0}, requestId:{1}, data: {2}", request.task, requestId, request.data);

                // get task handler name (with/without action)
                var taskHandlerPath = string.Empty;
                if (string.IsNullOrEmpty(actionImplementation))
                {
                    taskHandlerPath = request.task;
                }
                else
                {
                    taskHandlerPath = $"{request.task}.{actionImplementation}";
                }
                    
                var taskHandlerName = ApplicationConfiguration.CeleryRoutingConfiguration.GetHandler(taskHandlerPath);
                if (string.IsNullOrEmpty(taskHandlerName))
                {
                    response.status = "failure";
                    response.reason = $"TaskHandler '{taskHandlerPath}' does not exist in TCM configuration";
                    log.Error($"AddTask fail because {response.reason}.");
                    return response;
                }

                log.Debug($"Info - Request: {request.task} should be handled by taskHandlerName: {taskHandlerName}.");

                var taskHandlerType = Type.GetType($"{taskHandlerName}.TaskHandler, {taskHandlerName}");
                var taskHandler = (ITaskHandler)Activator.CreateInstance(taskHandlerType);

                response.retval = taskHandler.HandleTask(request.data);
                response.status = "success";

                log.Debug($"Info - Add Task Request Success: {request.task}");
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