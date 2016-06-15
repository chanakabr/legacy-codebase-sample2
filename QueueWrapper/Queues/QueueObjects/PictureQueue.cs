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
            //the "picture" parameter will ensure that the config values are the ones relevent for the PictureQueue 
            //the "true" indicates to set the content type of the message in the properties of the message
            this.Implementation = new RabbitQueue(ConfigType.PictureConfig, true);
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
