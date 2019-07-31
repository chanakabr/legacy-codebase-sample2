using System;
using System.Collections.Generic;
using System.Reflection;
using QueueWrapper;
using ApiObjects;
using KLogMonitor;

namespace RabbitQueueWrapper
{
    public class Handler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static string CeleryDateFormat
        {
            get { return BaseCeleryData.CELERY_DATE_FORMAT; }
        }

        /// <summary>
        /// Enqueue messages to RabbitMQ
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="task">task name in Celery, for example: "distributed_tasks.process_renew_subscription"</param>
        /// <param name="routingKey">routing key name in RabbitMQ, for example: "PROCESS_RENEW_SUBSCRIPTION\1483"</param>
        /// <param name="objectToEnqueue"></param>
        /// <param name="eta">eta in CeleryDateFormat</param>
        /// <param name="extraArgs"></param>
        /// <exception cref="ArgumentNullException">task, routingKey or objectToEnqueue is null</exception>
        /// <returns>true if the object was Enqueue successfully; otherwise, false.</returns>
        public static bool Enqueue(int groupId, string task, string routingKey, object objectToEnqueue, DateTime? eta = null, List<object> extraArgs = null)
        {
            bool enqueueSuccessful = false;

            if (string.IsNullOrEmpty(task))
            {
                log.Error("Enqueue - \"queue\" could not be empty.");
                throw new ArgumentNullException("task");
            }

            if (string.IsNullOrEmpty(routingKey))
            {
                log.Error("Enqueue - \"routingKey\" could not be empty.");
                throw new ArgumentNullException("routingKey");
            }

            if (objectToEnqueue == null)
            {
                log.Error("Enqueue - \"objectToEnqueue\" could not be empty.");
                throw new ArgumentNullException("objectToEnqueue");
            }
            
            try
            {
                var data = new BaseCeleryData()
                {
                    id = Guid.NewGuid().ToString(),
                    task = task,
                    // primary args object is the notified object itself, it will be a complete json object
                    args = new List<object>()
                    {
                        groupId,
                        objectToEnqueue
                    },
                    GroupId = groupId
                };

                if (extraArgs != null && extraArgs.Count > 0)
                {
                    data.args.AddRange(extraArgs);
                }

                data.args.Add(data.RequestId);

                if (eta != null && eta.HasValue)
                {
                    data.eta = eta.Value.ToString(BaseCeleryData.CELERY_DATE_FORMAT);
                }

                var queue = new GenericCeleryQueue();
                enqueueSuccessful = queue.Enqueue(data, routingKey);
                if (enqueueSuccessful)
                {
                    log.DebugFormat("Success to enqueue object to RabbitMQ. data: {0}", data);
                }
                else
                {
                    log.ErrorFormat("Failed to enqueue object to RabbitMQ. data: {0}", data);
                }
            }
            catch (Exception ex)
            {
                enqueueSuccessful = false;
                log.Error(string.Format("An Exception was occurred in Enqueue. groupId:{0}, task:{1}, routingKey:{2}, objectToEnqueue:{3}, eta:{4}, extraArgs:{5}", 
                                        groupId, task, routingKey, objectToEnqueue.ToString(), eta, extraArgs != null ? string.Join(", ", extraArgs) : "null"), ex);
            }
            
            return enqueueSuccessful;
        }
    }
}
