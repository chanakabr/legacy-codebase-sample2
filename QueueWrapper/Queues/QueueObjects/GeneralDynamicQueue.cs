using QueueWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueWrapper.Queues.QueueObjects
{
    public class GeneralDynamicQueue : BaseQueue
    {
        private string Name { get; set; }

        public GeneralDynamicQueue(string name)
            : base()
        {
            Name = name;

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

        public bool IsQueueExist()
        {
            return this.Implementation.IsQueueExist(Name);
        }

        public bool AddQueue(string routingKey)
        {
            return this.Implementation.AddQueue(Name, routingKey);
        }

        public bool DeleteQueue()
        {
            return this.Implementation.DeleteQueue(Name);
        }
    }
}
