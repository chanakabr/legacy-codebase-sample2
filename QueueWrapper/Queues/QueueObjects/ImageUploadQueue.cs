using QueueWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public class ImageUploadQueue : BaseQueue
    {

        public ImageUploadQueue()
            : base()
        {
            this.Implementation = new RabbitQueue(ConfigType.ImageUpload, true);
            storeForRecovery = true;
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
