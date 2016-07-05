using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
                    case ApiObjects.eSetupTask.NotificationCleanupIteration:
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

                        bool v1Success = true;
                        bool v2Success = true;

                        string urlV1 = TVinciShared.WS_Utils.GetTcmConfigValue("ES_URL_V1");
                        string urlV2 = TVinciShared.WS_Utils.GetTcmConfigValue("ES_URL_V2");

                        var worker = new StatisticsMigrationTool(urlV1, urlV2);
                        success = worker.Migrate(request.GroupID);

                        break;
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
