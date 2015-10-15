using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ExportHandler.WS_API;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using TVinciShared;

namespace ExportHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.InfoFormat("Export request. data={0}", data);

                // build request object
                ExportRequest request = JsonConvert.DeserializeObject<ExportRequest>(data);

                if (request == null)
                    throw new Exception(string.Format("Failed to desterilized export request. data = {0}", data != null ? data : string.Empty));

                string url = WS_Utils.GetTcmConfigValue("WS_API");
                string username = string.Empty;
                string password = string.Empty;
                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupId, ref username, ref password, ApiObjects.eWSModules.API);

                API apiClient = new API();
                if (!string.IsNullOrEmpty(url))
                    apiClient.Url = url;

                bool success = apiClient.Export(username, password, request.TaskId, request.Version);

                if (!success)
                {
                    throw new Exception(string.Format("Export request failed. request.TaskId = {0}, request.Version = {1}",
                        request.TaskId,                                             // {0}
                        request.Version != null ? request.Version : string.Empty)); // {1}
                }

                result = "success";
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}
