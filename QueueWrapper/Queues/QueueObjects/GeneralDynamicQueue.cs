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

        public GeneralDynamicQueue(string name, ConfigType configType = ConfigType.DefaultConfig)
            : base()
        {
            Name = name;

            this.Implementation = new RabbitQueue(configType, true);
        }

        public override bool Enqueue(ApiObjects.QueueObject record, string sRouteKey, long expirationMiliSec = 0)
        {
            return base.Enqueue(record, sRouteKey, expirationMiliSec);
        }

        public override T Dequeue<T>(string sQueueName, out string sAckId)
        {
            return base.Dequeue<T>(sQueueName, out sAckId);
        }

        public bool IsQueueExist()
        {
            return this.Implementation.IsQueueExist(Name);
        }

        public bool AddQueue(string routingKey, long expirationMiliSec = 0)
        {
            return this.Implementation.AddQueue(Name, routingKey, expirationMiliSec);
        }

        public bool DeleteQueue()
        {
            return this.Implementation.DeleteQueue(Name);
        }
    }
}
