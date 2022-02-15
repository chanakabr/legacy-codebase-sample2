using Phx.Lib.Log;
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
using Phx.Lib.Appconfig;
using CachingProvider.LayeredCache;

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


                            success = worker.BuildIndex();

                            if (success)
                            {
                                LayeredCache.Instance.SetInvalidationKey(LayeredCacheConfigNames.GET_COUNTRY_BY_IP_INVALIDATION_KEY);
                                LayeredCache.Instance.SetInvalidationKey(LayeredCacheConfigNames.GET_PROXY_IP_INVALIDATION_KEY);
                            }

                            break;

                            #endregion
                        }
                    case ApiObjects.eSetupTask.NotificationSeriesCleanupIteration:
                    {
                        #region Notification Clean Iteration

                        //Call Notifications WCF service
                        var status = Core.Notification.Module.DeleteAnnouncementsOlderThan();
                        if (status != null && status.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                        {
                            log.Debug("NotificationCleanupIteration: Successfully run cleanup notifications");
                            success = true;
                        }
                        else
                            log.Error("NotificationCleanupIteration: Error received when trying to run cleanup notifications");

                        break; 

                        #endregion
                    }
                    case ApiObjects.eSetupTask.RecordingsCleanup:
                    {
                        #region Recordings Cleanup

                        success = Core.ConditionalAccess.Module.CleanupRecordings();

                        if (!success)
                        {
                            log.Error("CleanupRecordings failed");
                        }
                        else
                        {
                            log.Debug("CleanupRecordings finished successfully");
                        }

                        break;

                        #endregion
                    }
                 
                    case ApiObjects.eSetupTask.InsertExpiredRecordingsTasks:
                    {
                        #region Recording Lifetime

                        success = Core.ConditionalAccess.Module.HandleRecordingsLifetime();

                        if (!success)
                        {
                            log.Error("HandleRecordingsLifetime failed");
                        }
                        else
                        {
                            log.Debug("HandleRecordingsLifetime finished successfully");
                        }
                        break;
                        #endregion
                    }
                    case ApiObjects.eSetupTask.RecordingScheduledTasks:
                    {
                        #region Recordings Scheduled Tasks

                        success = Core.ConditionalAccess.Module.HandleRecordingsScheduledTasks();

                        if (!success)
                        {
                            log.Error("HandleRecordingsScheduledTasks failed");
                        }
                        else
                        {
                            log.Debug("HandleRecordingsScheduledTasks finished successfully");
                        }
                        break;
                        #endregion
                    }
                    case ApiObjects.eSetupTask.ReminderCleanupIteration:
                    {
                        #region Reminder Clean Iteration

                        //Call Notifications WCF service
                        var status = Core.Notification.Module.DeleteOldReminders();
                        if (status != null && status.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                        {
                            log.Debug("ReminderCleanupIteration: Successfully run cleanup reminders");
                            success = true;
                        }
                        else
                            log.Error("ReminderCleanupIteration: Error received when trying to run cleanup reminders");

                        break;

                        #endregion
                    }
                    case ApiObjects.eSetupTask.PurgeUsers:
                        {
                            #region Purge Users

                            //Call users service 
                            if (Core.Users.Module.Purge())
                            {
                                log.Debug("PurgeUsers: Successfully run purge");
                                success = true;
                            }
                            else
                                log.Error("PurgeUsers: Error received when trying to purge");

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
