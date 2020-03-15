using QueueWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper.Queues.QueueObjects
{
    public class ExportTasksQueue : BaseQueue
    {
        public ExportTasksQueue()
            : base()
        {
            this.Implementation = new RabbitQueue(ConfigType.DefaultConfig, true);
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
