using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigurationManager.Types
{
    public class RedisClientConfiguration : BaseConfig<MailerConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.RedisClientConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> HostName = new BaseValue<string>("hostname", "redis.service.consul", false, "redis hostname, default value is redis.service.consul");
        public BaseValue<int> Port = new BaseValue<int>("port", 6379, false, "redis port, default value is 6379");
    }
}
