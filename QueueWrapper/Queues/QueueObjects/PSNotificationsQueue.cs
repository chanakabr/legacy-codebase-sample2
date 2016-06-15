using QueueWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    /// <summary>
    /// Professional services notifications queue
    /// </summary>
    public class PSNotificationsQueue : BaseQueue
    {
        /// <summary>
        /// Initialize the queue with rabbit queue implementation
        /// </summary>
        public PSNotificationsQueue()
        {
            this.Implementation = new RabbitQueue(ConfigType.ProfessionalServicesNotificationsConfig, true);
        }

        public override bool Enqueue(ApiObjects.QueueObject record, string sRouteKey, long expirationMiliSec = 0)
        {
            return base.Enqueue(record, sRouteKey, expirationMiliSec);
        }

        public override T Dequeue<T>(string sQueueName, out string sAckId)
        {
            return base.Dequeue<T>(sQueueName, out sAckId);
        }
    }
}
