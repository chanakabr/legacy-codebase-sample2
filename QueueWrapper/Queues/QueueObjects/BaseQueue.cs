using ApiObjects;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Threading;

namespace QueueWrapper
{
    public abstract class BaseQueue : IQueueable
    {
        private static readonly KLogger log = new KLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 5;
        private const int RECOVERY_TTL_MONTH = 2;
        private IQueueImpl m_QueueImpl;
        public bool storeForRecovery = false;

        public BaseQueue()
        { }

        public virtual bool Enqueue(ApiObjects.QueueObject record, string routingKey, long expirationMiliSec = 0)
        {
            bool bIsEnqueueSucceeded = false;
            string sMessage = string.Empty;

            if (record != null)
            {
                sMessage = record.ToString();
                if (this.Implementation != null)
                {
                    bIsEnqueueSucceeded = this.Implementation.Enqueue(sMessage, routingKey, expirationMiliSec);
                    if (!bIsEnqueueSucceeded)
                    {
                        log.ErrorFormat("Failed inserting message: {0}, routingKey: {1}", sMessage, routingKey);
                    }
                    else
                    {
                        log.DebugFormat("Successfully inserted message: {0}, routingKey: {1}", sMessage, routingKey);
                    }
                }

                if (bIsEnqueueSucceeded && storeForRecovery)
                {
                    var celeryData = record as ApiObjects.BaseCeleryData;

                    if (celeryData != null && celeryData.ETA.HasValue)
                    {
                        InsertQueueMessage(celeryData.GroupId, celeryData.RecoveryMessageId, sMessage, routingKey, celeryData.ETA.Value, this.GetType().ToString());
                    }
                }
            }

            return bIsEnqueueSucceeded;
        }

        public virtual bool RecoverMessages(int groupId, string record, string routingKey, string type)
        {
            string logString = string.Format("Parameters: groupId {0}, record {1}, routingKey {2}, type {3}",
                        groupId,                                        // {0}
                        record != null ? record : string.Empty,         // {1}
                        routingKey != null ? routingKey : string.Empty, // {2}
                        type != null ? type : string.Empty);            // {3}

            bool bIsEnqueueSucceeded = false;

            if (!string.IsNullOrEmpty(record))
            {
                if (this.Implementation != null)
                    bIsEnqueueSucceeded = this.Implementation.Enqueue(record, routingKey);

                if (!bIsEnqueueSucceeded)
                    log.Error("Error while trying to insert message to queue. " + logString);
                else
                    log.Debug("Message inserted to queue. " + logString);
            }

            return bIsEnqueueSucceeded;
        }

        protected void InsertQueueMessage(int groupId, string messageId, string messageData, string routingKey, DateTime excutionDate, string type)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();

            while (limitRetries >= 0)
            {
                string docKey = GetGroupQueueMessageDocKey(groupId, messageId);

                MessageQueue mq = new MessageQueue()
                {
                    ExecutionDate = DateTimeToUnixTimestamp(excutionDate),
                    Id = docKey,
                    MessageData = messageData,
                    RoutingKey = routingKey,
                    Type = type
                };

                var res = cbManager.Set(docKey, mq, (uint)(excutionDate.AddMonths(RECOVERY_TTL_MONTH) - DateTime.UtcNow).TotalSeconds);

                if (!res)
                {
                    Thread.Sleep(r.Next(50));
                    limitRetries--;
                }
                else
                    break;
            }
        }

        private string GetGroupQueueMessageDocKey(int groupId, string id)
        {
            return string.Format("MessageRecovery_Group_{0}_Id_{1}", groupId, id);
        }

        private long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }

        public virtual T Dequeue<T>(string sQueueName, out string sAckId)
        {
            sAckId = string.Empty;

            T objectReturned = default(T);
            if (!string.IsNullOrEmpty(sQueueName))
            {
                objectReturned = this.Implementation.Dequeue<T>(sQueueName, out sAckId);
            }

            return objectReturned;
        }

        internal IQueueImpl Implementation
        {
            get { return this.m_QueueImpl; }
            set { this.m_QueueImpl = value; }
        }
    }
}
