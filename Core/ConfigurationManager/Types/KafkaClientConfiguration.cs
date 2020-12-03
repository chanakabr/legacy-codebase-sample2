using ConfigurationManager.ConfigurationSettings.ConfigurationBase;


namespace ConfigurationManager
{
    public class KafkaClientConfiguration : BaseConfig<KafkaClientConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.KafkaClientConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> BootstrapServers = new BaseValue<string>("bootstrapServers", "kafka.service.consul:9092", false, "Initial list of brokers as a CSV list of broker host or host:port");
        public BaseValue<int> SocketTimeoutMs = new BaseValue<int>("socketTimeoutMs", 60000, false, "Default timeout for network requests");
        public BaseValue<bool> ConsumerAutoCommit = new BaseValue<bool>("consumerAutoCommit", false)
        {
            EnvironmentVariable = "KAFKA_CONSUMER_AUTO_COMMIT"
        };
        public BaseValue<string> HealthCheckTopic = new BaseValue<string>("healthCheckTopic", "healthcheck-topic");
    }
}