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
using System.Web;
using System.ServiceModel;

namespace SeriesRecordingTaskHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";
            try
            {
                log.DebugFormat("starting SeriesRecordingTask request. data={0}", data);
                SeriesRecordingTaskRequest request = JsonConvert.DeserializeObject<SeriesRecordingTaskRequest>(data);
                string url = TVinciShared.WS_Utils.GetTcmConfigValue("WS_CAS");
                using (WS_CAS.module cas = new WS_CAS.module())
                {
                    cas.Timeout = 600000;
                    if (!string.IsNullOrEmpty(url))
                    {
                        cas.Url = url;
                    }
                    string username = string.Empty;
                    string password = string.Empty;

                    TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupId, ref username, ref password, ApiObjects.eWSModules.CONDITIONALACCESS);

                    switch (request.SeriesRecordingTaskType)
                    {
                        case ApiObjects.eSeriesRecordingTask.FirstFollower:
                            if (cas.HandleFirstFollowerRecording(username, password, request.UserId, request.DomainId, request.ChannelId, request.SeriesId, request.SeasonNumber))
                            {
                                result = "success";
                            }
                            else
                            {
                                throw new Exception(string.Format("FirstFollowerRecording request did not finish successfully, request: {0}", request.ToString()));
                            }
                            break;
                        case ApiObjects.eSeriesRecordingTask.CompleteRecordings:
                            if (cas.CompleteDomainSeriesRecordings(username, password, request.DomainId))
                            {
                                result = "success";
                            }
                            else
                            {
                                throw new Exception(string.Format("CompleteDomainSeriesRecordings request did not finish successfully, request: {0}", request.ToString()));
                            }
                            break;
                        default:
                            break;
                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}

namespace SeriesRecordingTaskHandler.WS_CAS
{
    // adding request ID to header
    public partial class module
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