using Confluent.Kafka;

namespace EventBus.Kafka
{

    public interface IKafkaProducerFactory<TKey, TVal>
    {
        IProducer<TKey, TVal> Build();
    }

    /// <summary>
    /// Implementing singleton threadsafe producer
    /// https://github.com/confluentinc/confluent-kafka-dotnet/issues/1096
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    public class KafkaProducerFactory<TKey, TVal> : IKafkaProducerFactory<TKey, TVal>
    {
        private static IProducer<TKey, TVal> _ProducerInstance;
        private static readonly object _Padlock = new object();

        private readonly ProducerConfig _Config;


        public KafkaProducerFactory(ProducerConfig config)
        {
            _Config = config;
        }

        public IProducer<TKey, TVal> Build()
        {
            if (_ProducerInstance == null)
            {
                lock (_Padlock)
                {
                    if (_ProducerInstance == null)
                    {
                        _ProducerInstance = new ProducerBuilder<TKey, TVal>(_Config).Build();
                    }
                }
            }
            return _ProducerInstance;
        }
    }
}
