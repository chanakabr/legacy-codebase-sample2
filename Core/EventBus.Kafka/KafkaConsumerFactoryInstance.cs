using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EventBus.Kafka.Helpers;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phx.Lib.Appconfig;

namespace EventBus.Kafka
{
    public class KafkaConsumerFactoryInstance
    {
        private readonly ConcurrentDictionary<IReadOnlyDictionary<string, string>, IKafkaConsumerFactory> _factory =
            new ConcurrentDictionary<IReadOnlyDictionary<string, string>, IKafkaConsumerFactory>(new DictionaryComparer());

        private readonly Lazy<IReadOnlyDictionary<string, string>> _defaultConfiguration =
            new Lazy<IReadOnlyDictionary<string, string>>(GetDefaultConfiguration, LazyThreadSafetyMode.PublicationOnly);

        private static readonly Lazy<KafkaConsumerFactoryInstance> LazyInstance =
            new Lazy<KafkaConsumerFactoryInstance>(() => new KafkaConsumerFactoryInstance(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static KafkaConsumerFactoryInstance Instance => LazyInstance.Value;

        private KafkaConsumerFactoryInstance() { }

        public IKafkaConsumerFactory Acquire(IReadOnlyDictionary<string, string> additionalConfig = null)
        {
            var config = additionalConfig != null ? ExtractConfiguration(additionalConfig) : _defaultConfiguration.Value;
            return _factory.GetOrAdd(config, KafkaConsumerFactoryInitializer);
        }

        private static IKafkaConsumerFactory KafkaConsumerFactoryInitializer(IReadOnlyDictionary<string, string> kafkaConfig)
        {
            // we should create a new dictionary, because it is being mutated inside OTT.Lib.Kafka...
            return new KafkaConsumerFactory(kafkaConfig.ToDictionary(_ => _.Key, _ => _.Value), LoggerFactory.Create(builder => builder.AddLog4Net()));
        }

        private IReadOnlyDictionary<string,string> ExtractConfiguration(IReadOnlyDictionary<string,string> additionalConfig)
        {
            // net48 doesn't have a constructor accepting IEnumerable<KeyValuePair>...
            var result = _defaultConfiguration.Value.ToDictionary(_ => _.Key, _=> _.Value);
            foreach (var item in additionalConfig)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value);
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<string, string> GetDefaultConfiguration()
        {
            var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
            var kafkaConfig = new Dictionary<string, string>
            {
                { KafkaConfigKeys.BootstrapServers, tcmConfig.BootstrapServers.Value },
                { KafkaConfigKeys.SocketTimeoutMs, tcmConfig.SocketTimeoutMs.Value.ToString() }
            };

            return kafkaConfig;
        }
    }
}
