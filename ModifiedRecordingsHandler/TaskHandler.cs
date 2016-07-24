using KLogMonitor;
using ModifiedRecordingsHandler.WS_CAS;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModifiedRecordingsHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";
            try
            {
                log.DebugFormat("starting ModifiedRecording request. data={0}", data);
                ModifiedRecordingRequest request = JsonConvert.DeserializeObject<ModifiedRecordingRequest>(data);
                HandleDomainQuataByRecordingTask expiredRecording = new HandleDomainQuataByRecordingTask()
                {
                    Id = request.Id,
                    RecordingId = request.RecordingId,
                    GroupId = request.GroupId,
                    ScheduledExpirationEpoch = request.ScheduledExpirationEpoch,
                    OldRecordingDuration = request.OldRecordingDuration
                };
                string url = TVinciShared.WS_Utils.GetTcmConfigValue("WS_CAS");
                using (WS_CAS.module cas = new WS_CAS.module())
                {
                    cas.Timeout = 600000;
                    if (!string.IsNullOrEmpty(url))
                    {
                        cas.Url = url;
                    }

                    if (cas.HandleDomainQuotaByRecording(expiredRecording))
                    {
                        result = "success";
                    }
                    else
                    {
                        throw new Exception(string.Format("ModifiedRecording request did not finish successfully, request: {0}", request.ToString()));
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


namespace ModifiedRecordingsHandler.WS_CAS
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