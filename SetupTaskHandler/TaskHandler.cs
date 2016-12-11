using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net;
using System.Web;
using System.ServiceModel;
using QueueWrapper;
using ApiObjects;

namespace SetupTaskHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting setup task. data={0}", data);

                SetupTaskRequest request = JsonConvert.DeserializeObject<SetupTaskRequest>(data);

                bool success = false;

                if (request.Mission == null || !request.Mission.HasValue)
                {
                    throw new Exception("Setup task received invalid task");
                }

                switch (request.Mission.Value)
                {
                    case ApiObjects.eSetupTask.BuildIPToCountry:
                    {
                        #region IP to Country

                        var worker = new IPToCountryIndexBuilder();

                        bool v1Success = true;
                        bool v2Success = true;

                        string urlV1 = TVinciShared.WS_Utils.GetTcmConfigValue("ES_URL_V1");
                        string urlV2 = TVinciShared.WS_Utils.GetTcmConfigValue("ES_URL_V2");

                        if (string.IsNullOrEmpty(urlV1) && string.IsNullOrEmpty(urlV2))
                        {
                            success = worker.BuildIndex();
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(urlV1))
                            {
                                v1Success = worker.BuildIndex(urlV1, 1);
                            }

                            if (!string.IsNullOrEmpty(urlV2))
                            {
                                v2Success = worker.BuildIndex(urlV2, 2);
                            }

                            success = v1Success && v2Success;
                        }

                        break;

                        #endregion
                    }
                    case ApiObjects.eSetupTask.NotificationSeriesCleanupIteration:
                    {
                        #region Notification Clean Iteration

                        //Call Notifications WCF service
                        string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("ws_notifications");
                        using (ws_notifications.NotificationServiceClient service = new ws_notifications.NotificationServiceClient())
                        {
                            if (!string.IsNullOrEmpty(sWSURL))
                                service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);
                            else
                                log.ErrorFormat("NotificationCleanupIteration: Couldn't find WS_Notifications URL");

                            var status = service.DeleteAnnouncementsOlderThan(string.Empty, string.Empty);
                            if (status != null && status.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                                log.Debug("NotificationCleanupIteration: Successfully run cleanup notifications");
                            else
                                log.Error("NotificationCleanupIteration: Error received when trying to run cleanup notifications");
                        }
                        break; 

                        #endregion
                    }
                    case ApiObjects.eSetupTask.RecordingsCleanup:
                    {
                        #region Recordings Cleanup

                        string url = TVinciShared.WS_Utils.GetTcmConfigValue("WS_CAS");
                        using (SetupTaskHandler.WS_ConditionalAccess.module cas = new SetupTaskHandler.WS_ConditionalAccess.module())
                        {
                            if (!string.IsNullOrEmpty(url))
                            {
                                cas.Url = url;
                            }

                            cas.Timeout = 600000;
                            success = cas.CleanupRecordings();

                            if (!success)
                            {
                                log.Error("CleanupRecordings failed");
                            }
                            else
                            {
                                log.Debug("CleanupRecordings finished successfully");
                            }
                        }

                        break;

                        #endregion
                    }
                    case ApiObjects.eSetupTask.MigrateStatistics:
                    {
                        #region Migrate Statistics

                        string urlV1 = TVinciShared.WS_Utils.GetTcmConfigValue("ES_URL_V1");
                        string urlV2 = TVinciShared.WS_Utils.GetTcmConfigValue("ES_URL_V2");

                        DateTime? startDate = null;

                        if (request.DynamicData.ContainsKey("START_DATE"))
                        {
                            string startDateString = request.DynamicData["START_DATE"].ToString();
                            DateTime temp;

                            DateTime.TryParseExact(startDateString, "yyyyMMddHHmmss",
                                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None,
                                out temp);

                            startDate = temp;
                        }

                        var worker = new StatisticsMigrationTool(urlV1, urlV2);
                        success = worker.Migrate(request.GroupID, startDate);

                        if (success)
                        {
                            var queue = new SetupTasksQueue();

                            var dynamicData = new Dictionary<string, object>();

                            if (startDate != null && startDate.HasValue)
                            {
                                dynamicData.Add("START_DATE", DateTime.UtcNow);
                            }

                            var queueObject = new CelerySetupTaskData(request.GroupID, eSetupTask.MigrateStatistics, dynamicData)
                            {
                                ETA = DateTime.UtcNow.AddMinutes(30)
                            };

                            try
                            {
                                queue.Enqueue(queueObject, "MIGRATE_STATISTICS");
                            }
                            catch (Exception ex)
                            {
                                log.Error("MigrateStatistics - " +
                                        string.Format("Error in MigrateStatistics: group = {0} ex = {1}, ST = {2}", request.GroupID, ex.Message, ex.StackTrace),
                                        ex);
                            }
                        }
                        break;

                        #endregion
                    }
                    case ApiObjects.eSetupTask.InsertExpiredRecordingsTasks:
                    {
                        #region Recording Lifetime
                        string casUrl = TVinciShared.WS_Utils.GetTcmConfigValue("WS_CAS");
                        using (SetupTaskHandler.WS_ConditionalAccess.module cas = new SetupTaskHandler.WS_ConditionalAccess.module())
                        {
                            if (!string.IsNullOrEmpty(casUrl))
                            {
                                cas.Url = casUrl;
                            }

                            cas.Timeout = 600000;
                            success = cas.HandleRecordingsLifetime();

                            if (!success)
                            {
                                log.Error("HandleRecordingsLifetime failed");
                            }
                            else
                            {
                                log.Debug("HandleRecordingsLifetime finished successfully");
                            }
                        }
                        break;
                        #endregion
                    }
                    case ApiObjects.eSetupTask.RecordingScheduledTasks:
                    {
                        #region Recordings Scheduled Tasks
                        string casUrl = TVinciShared.WS_Utils.GetTcmConfigValue("WS_CAS");
                        using (SetupTaskHandler.WS_ConditionalAccess.module cas = new SetupTaskHandler.WS_ConditionalAccess.module())
                        {
                            if (!string.IsNullOrEmpty(casUrl))
                            {
                                cas.Url = casUrl;
                            }

                            cas.Timeout = 600000;
                            success = cas.HandleRecordingsScheduledTasks();

                            if (!success)
                            {
                                log.Error("HandleRecordingsScheduledTasks failed");
                            }
                            else
                            {
                                log.Debug("HandleRecordingsScheduledTasks finished successfully");
                            }
                        }
                        break;
                        #endregion
                    }
                    case ApiObjects.eSetupTask.ReminderCleanupIteration:
                    {
                        #region Reminder Clean Iteration

                        //Call Notifications WCF service
                        string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("ws_notifications");
                        using (ws_notifications.NotificationServiceClient service = new ws_notifications.NotificationServiceClient())
                        {
                            if (!string.IsNullOrEmpty(sWSURL))
                                service.Endpoint.Address = new System.ServiceModel.EndpointAddress(sWSURL);
                            else
                                log.ErrorFormat("ReminderCleanupIteration: Couldn't find WS_Notifications URL");

                            var status = service.DeleteOldReminders(string.Empty, string.Empty);
                            if (status != null && status.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                                log.Debug("ReminderCleanupIteration: Successfully run cleanup reminders");
                            else
                                log.Error("ReminderCleanupIteration: Error received when trying to run cleanup reminders");
                        }
                        break;

                        #endregion
                    }
                    default:
                        break;
                }

                if (!success)
                {
                    throw new Exception(string.Format(
                        "Setup task on {0} did not finish successfully.", request.Mission.ToString()));
                }
                else
                {
                    result = "success";
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

namespace SetupTaskHandler.WS_ConditionalAccess
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
