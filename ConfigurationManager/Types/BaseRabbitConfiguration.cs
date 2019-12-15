using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public abstract class BaseRabbitConfiguration : BaseConfig<BaseRabbitConfiguration>
    {
        public BaseValue<string> HostName = new BaseValue<string>("hostName", "rabbit.service.consul", false, "RabbitMQ host name (server address). Only for 'default' it is mandatory.");
        public BaseValue<string> UserName = new BaseValue<string>("userName", "admin", true, "RabbitMQ login user. Only for 'default' it is mandatory.");
        public BaseValue<string> Password = new BaseValue<string>("password", TcmObjectKeys.Stub, true, "RabbitMQ login password. Only for 'default' it is mandatory.");
        public BaseValue<string> RoutingKey = new BaseValue<string>("routingKey", ".");
        public BaseValue<string> Exchange = new BaseValue<string>("exchange", "scheduled_tasks", false, "RabbitMQ exchange. Only for 'default' it is mandatory.");
        public BaseValue<string> Queue = new BaseValue<string>("queue", ".");
        public BaseValue<string> VirtualHost = new BaseValue<string>("virtualHost", "/");
        public BaseValue<string> ExchangeType = new BaseValue<string>("exchangeType", "topic");
        public BaseValue<int> Port = new BaseValue<int>("port", 5672, false, "RabbitMQ access port.");
        public BaseValue<int> Heartbeat = new BaseValue<int>("heartbeat", 20, false, "Heartbeat timeout in seconds. see: https://www.rabbitmq.com/heartbeats.html");



    }
}