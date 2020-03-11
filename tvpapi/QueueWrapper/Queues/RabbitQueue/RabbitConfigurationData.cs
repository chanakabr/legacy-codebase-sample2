using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public class RabbitConfigurationData
    {
        #region Properties

        public string Exchange {get; set;}
        public string QueueName { get; set; }
        public string RoutingKey { get; set; } 
        public string Host { get; set; }
        public string Password { get; set; }
        public string ExchangeType { get; set; }
        public string Port { get; set; }
        public string VirtualHost { get; set; }
        public string Username { get; set; }

        #endregion

        #region CTOR

        public RabbitConfigurationData(string sExchange, string sQueueName, string sRoutingKey, string sHost, string sPassword, string sExchangeType, string sVirtualPort, string sUsername, string sPort)
        {
            this.Exchange = sExchange;
            this.QueueName = sQueueName;
            this.RoutingKey = sRoutingKey;
            this.Host = sHost;
            this.Password = sPassword;
            this.ExchangeType = sExchangeType;
            this.Username = sUsername;
            this.Port = sPort;
            this.VirtualHost = sVirtualPort;
        }

        public RabbitConfigurationData()
        { }

        #endregion

    }
}
