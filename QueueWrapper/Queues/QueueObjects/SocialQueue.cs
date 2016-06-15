using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper.Queues.QueueObjects
{
    public class SocialQueue : BaseQueue
    {
        public SocialQueue()
        {
            this.Implementation = new RabbitQueue(Enums.ConfigType.SocialFeedConfig, true);
        }

        public override bool Enqueue(ApiObjects.QueueObject record, string sRouteKey, long expirationMiliSec = 0)
        {
            return base.Enqueue(record, sRouteKey, expirationMiliSec);
        }

        public override T Dequeue<T>(string sQueueName, out string sAckId)
        {
            sAckId = string.Empty;
            return base.Dequeue<T>(sQueueName, out sAckId);
        }
    }
}
