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


        public BaseValue<string> PersistentAddress = new BaseValue<string>(TcmObjectKeys.RedisPersistentAddress, "redis.service.consul:6379", false, "redis persistent hostname, default value is redis.service.consul:6379");
        public BaseValue<string> CacheAddress = new BaseValue<string>(TcmObjectKeys.RedisCacheAddress, "redis.service.consul:6379", false, "redis cache hostname, default value is redis.service.consul:6379");        
    }

}
