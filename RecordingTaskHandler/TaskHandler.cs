using ApiObjects;
using KLogMonitor;
using Newtonsoft.Json;
using RecordingTaskHandler.WS_CAS;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;
using System.Net;
using System.Web;
using System.ServiceModel;
using DAL;
using System.Threading;

namespace RecordingTaskHandler
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
                log.DebugFormat("starting recording task. data={0}", data);

                RecordingTaskRequest request = JsonConvert.DeserializeObject<RecordingTaskHandler.RecordingTaskRequest>(data);

                bool success = false;
                string message = string.Empty;

                //if (request.Task == null || !request.Task.HasValue)
                //{
                //    throw new Exception("Received invalid recording task");
                //}

                string url = WS_Utils.GetTcmConfigValue("WS_CAS");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupID, ref username, ref password, ApiObjects.eWSModules.CONDITIONALACCESS);

                module cas = new module();

                if (!string.IsNullOrEmpty(url))
                {
                    cas.Url = url;
                }

                log.DebugFormat("Trying to handle recording task. Task = {0}, recordingId = {1}, programId = {2}, URL = {3}",
                    request.Task, request.RecordingId, request.ProgramId, url);

                switch (request.Task)
                {
                    case eRecordingTask.GetStatusAfterProgramEnded:
                    {
                        var recording = cas.GetRecordingStatus(username, password, request.RecordingId);

                        if (recording == null)
                        {
                            message = "recording is null";
                        }
                        else if (recording.Status == null)
                        {
                            message = "status is null";
                        }
                        else if (recording.Status.Code != 0)
                        {
                            message = string.Format("Status code is {0} and message is {1}", recording.Status.Code, recording.Status.Message);
                        }
                        else
                        {
                            success = true;
                        }

                        break;
                    }
                    case eRecordingTask.Record:
                    {
                        var recording = cas.RecordRetry(username, password, request.RecordingId);

                        if (recording == null)
                        {
                            message = "recording is null";
                        }
                        else if (recording.Status == null)
                        {
                            message = "status is null";
                        }
                        else if (recording.Status.Code != 0)
                        {
                            message = string.Format("Status code is {0} and message is {1}", recording.Status.Code, recording.Status.Message);
                        }
                        else
                        {
                            success = true;
                        }

                        break;
                    }
                    case eRecordingTask.UpdateRecording:
                    {
                        long[] epgs = new long[]{request.ProgramId};
                        var status = cas.IngestRecording(username, password, epgs, WS_CAS.eAction.Update);

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
                    case eRecordingTask.DistributeRecording:
                    {
                        bool shouldDistributeRecordingSynchronously = TVinciShared.WS_Utils.GetTcmBoolValue("ShouldDistributeRecordingSynchronously");                        
                        if (shouldDistributeRecordingSynchronously)
                        {
                            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                            {
                                cas.Timeout = 500000;
                                success = cas.DistributeRecording(username, password, request.ProgramId, request.RecordingId, request.EpgStartDate);
                            }
                        }
                        else
                        {
                            RecordingTaskHandler.WS_CAS.KeyValuePair seriesAndSeasonNumber = cas.GetSeriesIdAndSeasonNumberByEpgId(username, password, request.ProgramId);
                            int seasonNumber = 0;
                            if (seriesAndSeasonNumber != null && !string.IsNullOrEmpty(seriesAndSeasonNumber.key) && int.TryParse(seriesAndSeasonNumber.value, out seasonNumber))
                            {
                                long maxDomainSeriesId = 0;
                                HashSet<long> domainSeriesIds = RecordingsDAL.GetSeriesFollowingDomainsIds(request.GroupID, seriesAndSeasonNumber.key, seasonNumber, ref maxDomainSeriesId);
                                while (domainSeriesIds != null && domainSeriesIds.Count > 0 && maxDomainSeriesId > -1)
                                {                                    
                                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                                    {
                                        module casAsync = new module();
                                        casAsync.Url = cas.Url;
                                        casAsync.DistributeRecordingWithDomainIdsAsync(username, password, request.ProgramId, request.RecordingId, request.EpgStartDate, domainSeriesIds.ToArray());
                                    }

                                    domainSeriesIds = RecordingsDAL.GetSeriesFollowingDomainsIds(request.GroupID, seriesAndSeasonNumber.key, seasonNumber, ref maxDomainSeriesId);
                                }
                                success = true;
                            }
                            else
                            {
                                success = false;
                            }
                        }
                        
                        break;
                    }
                    case eRecordingTask.CheckRecordingDuplicateCrids:
                    {
                        success = cas.CheckRecordingDuplicateCrids(username, password, request.RecordingId);
                        break;
                    }
                }

                if (!success)
                {
                    throw new Exception(string.Format(
                        "Recording task on {0} did not finish successfully. message is {1}", request.Task.ToString(), message));
                }
                else
                {
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Recording task handler error. data = {0}, message = {2}, ex = {1}", data, ex, ex.Message);
                throw ex;
            }

            return result;
        }

        #endregion
    }
}

namespace RecordingTaskHandler.WS_CAS
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