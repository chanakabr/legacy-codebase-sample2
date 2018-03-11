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

        private void Initialize()
        {
            // host name
            HostName = new StringConfigurationValue("hostName", this);

            // user name
            UserName = new StringConfigurationValue("userName", this);

            // password
            Password = new StringConfigurationValue("password", this);

            // port
            Port = new NumericConfigurationValue("port", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 5672
            };

            // routing key
            RoutingKey = new StringConfigurationValue("routingKey", this)
            {
                ShouldAllowEmpty = true
            };

            // exchange
            Exchange = new StringConfigurationValue("exchange", this);

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