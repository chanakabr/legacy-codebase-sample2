using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using UserTaskHandler.WS_CAS;
using ApiObjects;

namespace UserTaskHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region ITaskHandler Members

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting user task. data={0}", data);

                UserTaskRequest request = JsonConvert.DeserializeObject<UserTaskRequest>(data);

                bool success = false;
                string message = string.Empty;
                
                string url = WS_Utils.GetTcmConfigValue("WS_CAS");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupID, ref username, ref password, ApiObjects.eWSModules.CONDITIONALACCESS);

                module cas = new module();

                if (!string.IsNullOrEmpty(url))
                {
                    cas.Url = url;
                }

                log.DebugFormat("Trying to handle user task. Task = {0}, userId = {1}, domainId= {2}, URL = {3}",
                    request.Task, request.UserId, request.DomainId, url);

                switch (request.Task)
                {
                    case ApiObjects.UserTaskType.Delete:
                    {
                        var status = cas.HandleUserTask(username, password, request.DomainId, request.UserId, WS_CAS.UserTaskType.Delete);

                        if (status == null)
                        {
                            message = "status is null";
                        }
                        else if (status.Code != 0)
                        {
                            message = string.Format("Status code is {0} and message is {1}", status.Code, status.Message);
                        }
                        else
                        {
                            success = true;
                        }

                        break;
                    }
                }

                if (!success)
                {
                    throw new Exception(string.Format(
                        "User task on {0} did not finish successfully. message is {1}", request.Task.ToString(), message));
                }
                else
                {
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("User task handler error. data = {0}, message = {2}, ex = {1}", data, ex, ex.Message);
                throw ex;
            }

            return result;
        }

        #endregion
    }
}

namespace UserTaskHandler.WS_CAS
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}

