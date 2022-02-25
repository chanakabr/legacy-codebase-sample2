using System.Collections.Generic;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

namespace EventBus.Kafka
{
    public static class KafkaProducerFactoryInstance
    {
        private static readonly object Lock = new object();
        private static readonly KLogger Logger = new KLogger(nameof(KafkaProducerFactoryInstance));

        private static IKafkaProducerFactory _factory;

        public static IKafkaProducerFactory Get()
        {
            if (_factory == null)
            {
                lock (Lock)
                {
                    if (_factory == null)
                    {
                        var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;

                        CreateHealthCheckTopic(tcmConfig.BootstrapServers.Value, tcmConfig.HealthCheckTopic.Value);

                        var kafkaConfig = new Dictionary<string, string>
                        {
                            { KafkaConfigKeys.BootstrapServers, tcmConfig.BootstrapServers.Value },
                            { KafkaConfigKeys.SocketTimeoutMs, tcmConfig.SocketTimeoutMs.Value.ToString() },
                            { KafkaConfigKeys.ClientId, KLogger.GetServerName() }
                        };

                        var loggerFactory = LoggerFactory.Create(builder => builder.AddLog4Net());

                        var clientFactory = new KafkaProducerClientFactory(kafkaConfig, loggerFactory.CreateLogger<IKafkaProducerClientFactory>());
                        _factory = new KafkaProducerFactory(clientFactory, loggerFactory);
                    }
                }
            }

            return _factory;
        }

        private static void CreateHealthCheckTopic(string bootstrapServers, string healthCheckTopic)
        {
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build())
            {
                try
                {
                    adminClient.CreateTopicsAsync(new[]
                    {
                        new TopicSpecification
                        {
                            Name = healthCheckTopic,
                            ReplicationFactor = 1,
                            NumPartitions = 1
                        }
                    });
                }
                catch (CreateTopicsException e)
                {
                    Logger.Error($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}", e);
                }
            }
        }
    }
}