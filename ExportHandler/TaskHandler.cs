using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
                log.DebugFormat("Export request. data={0}", data);

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

                apiClient.ExportAsync(username, password, request.TaskId, request.Version);

                result = "success";
                //if (!success)
                //{
                //    throw new Exception(string.Format("Export request failed. request.TaskId = {0}, request.Version = {1}",
                //        request.TaskId,                                             // {0}
                //        request.Version != null ? request.Version : string.Empty)); // {1}
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}


namespace ExportHandler.WS_API
{
    // adding request ID to header
    public partial class API
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);

            if (KLogMonitor.KLogger.AppType == KLogEnums.AppType.WCF)
            {
                if (request.Headers != null &&
                request.Headers[Constants.REQUEST_ID_KEY] == null &&
                OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(Constants.REQUEST_ID_KEY, OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY].ToString());
                }
            }
            else
            {
                if (request.Headers != null &&
                request.Headers[Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(Constants.REQUEST_ID_KEY, HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString());
                }
            }
            return request;
        }
    }
}
