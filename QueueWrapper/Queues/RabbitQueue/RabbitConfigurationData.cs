using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public class RabbitConfigurationData
    {
        public string Exchange { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public string ExchangeType { get; set; }
        public string Port { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string ContentType { get; set; }
        public ushort Heartbeat { get; set; }

        public RabbitConfigurationData(string exchange, string queueName, string sRoutingKey, 
            string host, string password, string exchangeType, string virtualPort, string username, string port, string contentType = "")
        {
            this.Exchange = exchange;
            this.QueueName = queueName;
            this.RoutingKey = sRoutingKey;
            this.Host = host;
            this.Password = password;
            this.ExchangeType = exchangeType;
            this.Username = username;
            this.Port = port;
            this.VirtualHost = virtualPort;
            this.ContentType = contentType;
        }

        public RabbitConfigurationData()
        {
        }
    }
}
