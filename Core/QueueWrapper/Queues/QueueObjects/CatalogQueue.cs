using ApiObjects;
using ApiObjects.MediaIndexingObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public class CatalogQueue : BaseQueue
    {
        public CatalogQueue(bool isLegacy = false)
        {
            Enums.ConfigType config = Enums.ConfigType.IndexingDataConfig;

            if (isLegacy)
            {
                config = Enums.ConfigType.DefaultConfig;
            }

            this.Implementation = new RabbitQueue(config, true);
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
