using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledTasks
{
    public class BaseScheduledTaskLastRunDetails: ScheduledTaskLastRunDetails
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public BaseScheduledTaskLastRunDetails() : base() { }

        public BaseScheduledTaskLastRunDetails(ScheduledTaskType scheduledTaskType)
            : base()
        {
            this.ScheduledTaskType = scheduledTaskType;
        }

        public BaseScheduledTaskLastRunDetails(DateTime lastRunDate, int impactedItems, double nextRunIntervalInSeconds, ScheduledTaskType scheduledTaskType)
            : base(lastRunDate, impactedItems, nextRunIntervalInSeconds, scheduledTaskType) { }

        private string GetKey()
        {
            string key = string.Empty;
            switch (ScheduledTaskType)
            {
                case ApiObjects.ScheduledTaskType.recordingsLifetime:
                    key = "recordings_lifetime";
                    break;
                case ApiObjects.ScheduledTaskType.recordingsScheduledTasks:
                    key = "recordings_scheduledTasks";
                    break;
                case ApiObjects.ScheduledTaskType.recordingsCleanup:
                    key = "recordings_cleanup";
                    break;
                case ApiObjects.ScheduledTaskType.notificationCleanup:
                    key = "notification_cleanup";
                    break;
                default:
                    break;
            }

            return key;
        }

        public override ScheduledTaskLastRunDetails GetLastRunDetails()
        {
            BaseScheduledTaskLastRunDetails response = null;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string scheduledTaksKey = GetKey();
            if (string.IsNullOrEmpty(scheduledTaksKey))
            {
                log.ErrorFormat("Failed GetKeyByType for scheduledTaskName: {0}", ScheduledTaskType.ToString());
                return response;
            }
            try
            {
                int numOfRetries = 0;
                while (numOfRetries < limitRetries)
                {
                    response = cbClient.Get<BaseScheduledTaskLastRunDetails>(scheduledTaksKey, out getResult);
                    if (getResult == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        log.ErrorFormat("Error while trying to get last scheduled task run details, KeyNotFound. scheduleTaskName: {0}, key: {1}", ScheduledTaskType.ToString(), scheduledTaksKey);
                        break;
                    }
                    else if (getResult == Couchbase.IO.ResponseStatus.Success)
                    {
                        log.DebugFormat("BaseScheduledTaskLastRunDetails with scheduleTaskName: {0} and key {1} was found", ScheduledTaskType.ToString(), scheduledTaksKey);
                        break;
                    }
                    else
                    {
                        log.ErrorFormat("Retrieving BaseScheduledTaskLastRunDetails with scheduledTaskName: {0} and key {1} failed with status: {2}, retryAttempt: {3}, maxRetries: {4}", ScheduledTaskType.ToString(), scheduledTaksKey, getResult, numOfRetries, limitRetries);
                        numOfRetries++;
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get last run details for BaseScheduledTaskLastRunDetails, scheduledTaskName: {0}, ex: {1}", ScheduledTaskType.ToString(), ex);
            }

            return response;
        }

        public override bool SetLastRunDetails()
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string scheduledTaksKey = GetKey();
            if (string.IsNullOrEmpty(scheduledTaksKey))
            {
                log.ErrorFormat("Failed GetKeyByType for scheduledTaskName: {0}", ScheduledTaskType.ToString());
                return false;
            }
            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    ulong version;
                    Couchbase.IO.ResponseStatus status;
                    BaseScheduledTaskLastRunDetails currentScheduledTask = cbClient.GetWithVersion<BaseScheduledTaskLastRunDetails>(scheduledTaksKey, out version, out status);
                    if (status == Couchbase.IO.ResponseStatus.Success || status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        result = cbClient.SetWithVersion<BaseScheduledTaskLastRunDetails>(scheduledTaksKey, this, version);
                    }

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while updating scheduled task run details. scheduledTaskName: {0}, number of tries: {1}/{2}",
                                         ScheduledTaskType.ToString(), numOfRetries, limitRetries);

                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating scheduled task run details, scheduledTaskName: {0}, ex: {1}", ScheduledTaskType.ToString(), ex);
            }

            return result;
        }

        public override bool SetNextRunIntervalInSeconds(double updatedNextRunIntervalInSeconds)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string scheduledTaksKey = GetKey();
            if (string.IsNullOrEmpty(scheduledTaksKey))
            {
                log.ErrorFormat("Failed GetKeyByType for scheduledTaskName: {0}", ScheduledTaskType.ToString());
                return false;
            }
            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    ulong version;
                    Couchbase.IO.ResponseStatus status;
                    BaseScheduledTaskLastRunDetails scheduledTask = cbClient.GetWithVersion<BaseScheduledTaskLastRunDetails>(scheduledTaksKey, out version, out status);
                    if (status == Couchbase.IO.ResponseStatus.Success)
                    {
                        scheduledTask.NextRunIntervalInSeconds = updatedNextRunIntervalInSeconds;
                        result = cbClient.SetWithVersion<BaseScheduledTaskLastRunDetails>(scheduledTaksKey, scheduledTask, version);
                    }
                    else if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        break;
                    }

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while updating scheduled task next run interval. scheduledTaskName: {0}, number of tries: {1}/{2}",
                                         ScheduledTaskType.ToString(), numOfRetries, limitRetries);

                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating scheduled task next run interval, scheduledTaskName: {0}, ex: {1}", ScheduledTaskType.ToString(), ex);
            }

            return result;
        }        

    }
}
