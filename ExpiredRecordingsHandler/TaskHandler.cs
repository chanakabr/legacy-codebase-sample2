using ExpiredRecordingsHandler.WS_CAS;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.ServiceModel;

namespace ExpiredRecordingsHandler
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
                ExpiredRecordingRequest request = JsonConvert.DeserializeObject<ExpiredRecordingRequest>(data);
                ExpiredRecordingScheduledTask expiredRecording = new ExpiredRecordingScheduledTask()
                {
                    Id = request.Id,
                    RecordingId = request.RecordingId,
                    GroupId = request.GroupId,
                    ScheduledExpirationEpoch = request.RecordingExpirationEpoch,
                    ScheduledExpirationDate = request.RecordingExpirationDate
                };
                string url = TVinciShared.WS_Utils.GetTcmConfigValue("WS_CAS");
                using (WS_CAS.module cas = new WS_CAS.module())
                {
                    cas.Timeout = 600000;
                    if (!string.IsNullOrEmpty(url))
                    {
                        cas.Url = url;
                    }

                    if (cas.HandleExpiredRecording(expiredRecording))
                    {
                        result = "success";
                    }
                    else
                    {
                        throw new Exception(string.Format("ExpiredRecording request did not finish successfully, request: {0}", request.ToString()));
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


namespace ExpiredRecordingsHandler.WS_CAS
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