using System.Collections.Generic;
using OTT.Lib.Kafka;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

namespace Phoenix.AsyncHandler.Kafka
{
    public static class KafkaConfig
    {
        public const string KafkaGroupId = "ott-service-phoenix-async-handler";

        public static string GetConsumerGroup(string kafkaGroupSuffix) => $"{KafkaConfig.KafkaGroupId}-{kafkaGroupSuffix}";
        
        public static IDictionary<string, string> Get()
        {
            var tcmConfig = ApplicationConfiguration.Current.KafkaClientConfiguration;
            var socketTimeout = tcmConfig.SocketTimeoutMs.Value != 0
                ? tcmConfig.SocketTimeoutMs.Value
                : tcmConfig.SocketTimeoutMs.GetDefaultValue();
            var config = new Dictionary<string, string>
            {
                { KafkaConfigKeys.BootstrapServers, tcmConfig.BootstrapServers.Value },
                { KafkaConfigKeys.SocketTimeoutMs, socketTimeout.ToString() },
                { KafkaConfigKeys.ClientId, KLogger.GetServerName() },
                { KafkaConfigKeys.EnableAutoCommit, "false" }
            };

            return config;
        }
    }
}
