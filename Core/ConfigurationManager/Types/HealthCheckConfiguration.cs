using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json;
using System;

namespace ConfigurationManager
{
    /*
     * TCM Example:
    health_check: 
    - Type: ElasticSearch
    - Type: CouchBase
    - Type: RabbitMQ
    - Type: SQL
    - Type: ThirdParty
    - Args:
      - {name}
      - {url}
     */

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class HealthCheckDefinition
    {
        [JsonProperty()]
        public HealthCheckType Type
        {
            get;
            set;
        }

        [JsonProperty()]
        public object[] Args
        {
            get;
            set;
        }
    }

    public enum HealthCheckType
    {
        SQL,
        CouchBase,
        ElasticSearch,
        ElasticSearch_7_13,
        RabbitMQ,
        ThirdParty,
        CacheRedis,
        Kafka,
        PersistentRedis,
    }
}
