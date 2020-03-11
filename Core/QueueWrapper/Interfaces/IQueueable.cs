using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.MediaIndexingObjects;
using ApiObjects;

namespace QueueWrapper
{
    public interface IQueueable
    {
        bool Enqueue(QueueObject record, string sRouteKey, long expirationMiliSec = 0);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Represents an object which will be dequeued from the queue. <paramref name=">QueueObject"/></typeparam>
        /// <param name="sQueueName"></param>
        /// <returns>Queue Object</returns>
        T Dequeue<T>(string sQueueName, out string sAckId);

        bool RecoverMessages(int groupId, string record, string routingKey, string type);
    }
}
