using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.MediaIndexingObjects;
using QueueWrapper.Enums;
using ApiObjects;

namespace QueueWrapper.Queues.QueueObjects
{
    public class EPGQueue : BaseQueue
    {

        public EPGQueue()
        {
            this.Implementation = new RabbitQueue(ConfigType.EPGConfig, true);    
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
