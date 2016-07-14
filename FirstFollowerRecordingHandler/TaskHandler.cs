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

namespace FirstFollowerRecordingHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";
            try
            {
                log.DebugFormat("starting ExpiredRecording request. data={0}", data);
                FirstFollowerRecordingRequest request = JsonConvert.DeserializeObject<FirstFollowerRecordingRequest>(data);
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
                    if (cas.HandleFirstFollowerRecording(username, password, request.DomainId, request.ChannelId, request.SeriesId, request.SeasonNumber))
                    {
                        result = "success";
                    }
                    else
                    {
                        throw new Exception(string.Format("FirstFollowerRecording request did not finish successfully, request: {0}", request.ToString()));
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


namespace FirstFollowerRecordingHandler.WS_CAS
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);

            if (request.Headers != null &&
                request.Headers[Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
            {
                request.Headers.Add(Constants.REQUEST_ID_KEY, HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString());
            }
            return request;
        }
    }
}