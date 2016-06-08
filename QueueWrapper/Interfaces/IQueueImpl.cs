using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public interface IQueueImpl : IDisposable
    {
        bool Enqueue(string message, string routingKey);

        T Dequeue<T>(string queueName, out string ackId);

        bool Ack(string queueName, string ackId);

        bool IsQueueExist(string name);

        bool AddQueue(string name, string routingKey);

        bool DeleteQueue(string name);
    }
}
