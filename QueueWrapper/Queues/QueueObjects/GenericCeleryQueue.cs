using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public class GenericCeleryQueue : BaseQueue
    {
        public GenericCeleryQueue()
        {
            this.Implementation = new RabbitQueue(Enums.ConfigType.DefaultConfig, true);
        }

        public override bool Enqueue(QueueObject record, string sRouteKey, long expirationMiliSec = 0)
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
