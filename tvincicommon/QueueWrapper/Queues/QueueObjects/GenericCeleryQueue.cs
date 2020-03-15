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

        public override bool Enqueue(QueueObject record, string sRouteKey)
        {
            return base.Enqueue(record, sRouteKey);
        }

        public override T Dequeue<T>(string sQueueName, out string sAckId)
        {
            sAckId = string.Empty;
            return base.Dequeue<T>(sQueueName, out sAckId);
        }
    }
}
