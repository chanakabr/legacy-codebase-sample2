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
        /// Initialize the queue with rabbit queue implemanation
        /// </summary>
        public PSNotificationsQueue()
        {
            this.Implementation = new RabbitQueue(ConfigType.ProfessionalServicesNotificationsConfig, true);
        }

        public override bool Enqueue(ApiObjects.QueueObject record, string sRouteKey)
        {
            return base.Enqueue(record, sRouteKey);
        }

        public override T Dequeue<T>(string sQueueName, out string sAckId)
        {
            return base.Dequeue<T>(sQueueName, out sAckId);
        }
    }
}
