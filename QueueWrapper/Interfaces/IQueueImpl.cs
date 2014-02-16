using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public interface IQueueImpl : IDisposable
    {
        bool Enqueue(string sMessage, string sRouteKey);

        T Dequeue<T>(string sQueueName, out string sAckId);

        bool Ack(string sQueueName, string sAckId);
    }
}
