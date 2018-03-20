using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class BaseRabbitConfiguration : ConfigurationValue
    {
        public StringConfigurationValue HostName;
        public StringConfigurationValue UserName;
        public StringConfigurationValue Password;
        public NumericConfigurationValue Port;
        public StringConfigurationValue RoutingKey;
        public StringConfigurationValue Exchange;
        public StringConfigurationValue Queue;
        public StringConfigurationValue VirtualHost;
        public StringConfigurationValue ExchangeType;

        public BaseRabbitConfiguration(string key) : base(key)
        {
            Initialize();
        }

        public BaseRabbitConfiguration(string key, ConfigurationValue parent) : base(key, parent)
        {
            this.Initialize();
        }

        protected virtual void Initialize()
        {
            // host name
            HostName = new StringConfigurationValue("hostName", this)
            {
                Description = "RabbitMQ host name (server address). Only for 'default' it is mandatory."
            };

            // user name
            UserName = new StringConfigurationValue("userName", this)
            {
                Description = "RabbitMQ login user. Only for 'default' it is mandatory."
            };

            // password
            Password = new StringConfigurationValue("password", this)
            {
                Description = "RabbitMQ login password. Only for 'default' it is mandatory."
            };

            // port
            Port = new NumericConfigurationValue("port", this)
            {
                Description = "RabbitMQ access port.",
                ShouldAllowEmpty = true,
                DefaultValue = 5672
            };

            // routing key
            RoutingKey = new StringConfigurationValue("routingKey", this)
            {
                ShouldAllowEmpty = true
            };

            // exchange
            Exchange = new StringConfigurationValue("exchange", this)
            {
                Description = "RabbitMQ exchange. Only for 'default' it is mandatory."
            };

            // queue
            Queue = new StringConfigurationValue("queue", this)
            {
                ShouldAllowEmpty = true
            };

            // virtual host
            VirtualHost = new StringConfigurationValue("virtualHost", this)
            {
                ShouldAllowEmpty = true
            };

            // exchange type
            ExchangeType = new StringConfigurationValue("exchangeType", this)
            {
                ShouldAllowEmpty = true
            };
        }

        public void CopyBaseValues(BaseRabbitConfiguration source)
        {
            this.HostName.ShouldAllowEmpty = true;
            this.UserName.ShouldAllowEmpty = true;
            this.Password.ShouldAllowEmpty = true;
            this.Port.ShouldAllowEmpty = true;
            this.Exchange.ShouldAllowEmpty = true;

            if (!this.HostName.Validate())
            {
                this.HostName.ObjectValue = source.HostName.ObjectValue;
            }

            if (!this.UserName.Validate())
            {
                this.UserName.ObjectValue = source.UserName.ObjectValue;
            }

            if (!this.Password.Validate())
            {
                this.Password.ObjectValue = source.Password.ObjectValue;
            }

            if (!this.Port.Validate())
            {
                this.Port.ObjectValue = source.Port.ObjectValue;
            }

            if (!this.Exchange.Validate())
            {
                this.Exchange.ObjectValue = source.Exchange.ObjectValue;
            }
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= HostName.Validate();
            result &= UserName.Validate();
            result &= Password.Validate();
            result &= Port.Validate();
            result &= RoutingKey.Validate();
            result &= Exchange.Validate();
            result &= Queue.Validate();
            result &= VirtualHost.Validate();
            result &= ExchangeType.Validate();

            return result;
        }
    }
}