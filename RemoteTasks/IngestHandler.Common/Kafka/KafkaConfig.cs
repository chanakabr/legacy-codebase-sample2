using System.Collections.Generic;
using OTT.Lib.Kafka;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

namespace IngestHandler.Common.Kafka
{
    public static class KafkaConfig
    {
        public static IDictionary<string, string> Get()
        {
            var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
            var config = new Dictionary<string, string>
            {
                { KafkaConfigKeys.BootstrapServers, tcmConfig.BootstrapServers.Value },
                { KafkaConfigKeys.SocketTimeoutMs, tcmConfig.SocketTimeoutMs.Value.ToString() },
                { KafkaConfigKeys.ClientId, KLogger.GetServerName() },
                { KafkaConfigKeys.AutoOffsetReset, "earliest" },
                { KafkaConfigKeys.AllowAutoCreateTopics, "true" },
                { KafkaConfigKeys.EnableAutoCommit, "false" }
            };

            return config;
        }
    }
}