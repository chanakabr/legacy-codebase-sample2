using System;
using System.IO;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Phx.Lib.Log;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Phx.Lib.Appconfig;
using RemoteTasksService.Infrastructure;
using RemoteTasksCommon;
using Counter = OTT.Lib.Metrics.Metrics;

namespace RemoteTasksService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service : IService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [WebInvoke(Method = "GET", UriTemplate = "metrics", BodyStyle = WebMessageBodyStyle.Bare)]
        public async Task<Stream> GetMetrics()
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
            return await Counter.CollectAsStreamAsync();
        }

        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "tasks")]
        public AddTaskResponse AddTask(AddTaskRequest request)
        {
            var response = new AddTaskResponse();
            var taskHandlerName = "unknown";
            var groupId = 0;

            try
            {
                var actionImplementation = string.Empty;
                var requestId = string.Empty;
                if (request != null)
                {
                    MonitorLogsHelper.UpdateHeaderData(Constants.REQUEST_ID_KEY, Guid.NewGuid().ToString());

                    // update request ID & group ID
                    if (ExtractRequestParams(request.data, ref requestId, ref groupId) && !MonitorLogsHelper.UpdateHeaderData(Constants.REQUEST_ID_KEY, requestId))
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

                taskHandlerName = ApplicationConfiguration.Current.CeleryRoutingConfiguration.GetHandler(taskHandlerPath);
                if (string.IsNullOrEmpty(taskHandlerName))
                {
                    response.status = "failure";
                    response.reason = $"TaskHandler '{taskHandlerPath}' does not exist in TCM configuration";
                    log.Error($"AddTask fail because {response.reason}.");
                    Metrics.Track(taskHandlerName, response, groupId);
                    return response;
                }

                log.Debug($"Info - Request: {request.task} should be handled by taskHandlerName: {taskHandlerName}.");

                var taskHandlerType = Type.GetType($"{taskHandlerName}.TaskHandler, {taskHandlerName}");

                if (taskHandlerType == null)
                {
                    response.status = "failure";
                    response.reason = $"TaskHandler '{taskHandlerName}' failed to load";
                    Metrics.Track(taskHandlerName, response, groupId);
                    return response;
                }

                var taskHandler = (ITaskHandler)Activator.CreateInstance(taskHandlerType);

                response.retval = taskHandler.HandleTask(request.data);
                response.status = "success";

                log.Debug($"Info - Add Task Request Success: {request.task}");
            }
            catch (Exception ex)
            {
                log.Error($"Error - Add Task {taskHandlerName} Request Failed", ex);
                response.status = "failure";
                response.reason = ex.Message;
            }

            Metrics.Track(taskHandlerName, response, groupId);
            return response;
        }

        private bool ExtractRequestParams(string messageData, ref string requestId, ref int groupId)
        {
            var status = false;
            try
            {
                requestId = string.Empty;
                groupId = 0;
                if (messageData != null)
                {
                    var reqContainer = JsonConvert.DeserializeObject<RequestParams>(messageData);

                    if (reqContainer != null && !string.IsNullOrEmpty(reqContainer.RequestId))
                    {
                        requestId = reqContainer.RequestId;
                        status = true;
                        int.TryParse(reqContainer.GroupId, out groupId);//parse only if request is valid
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error extracting request ID. messageData: {messageData}, ex: {ex}");
            }

            return status;
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
