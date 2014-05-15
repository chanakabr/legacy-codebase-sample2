using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using ApiObjects.MediaIndexingObjects;
using QueueWrapper.Enums;

namespace QueueWrapper
{
    public class PictureQueue : BaseQueue
    {
        public PictureQueue()
        {
            this.Implementation = new RabbitQueue(ConfigType.PictureConfig);//the parameter will ensure that the config values are the ones relevent for the PictureQueue
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
