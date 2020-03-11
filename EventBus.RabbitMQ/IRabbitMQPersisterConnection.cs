using System;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ
{
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        bool TryConnect();

        IModel CreateModel();
    }
}
