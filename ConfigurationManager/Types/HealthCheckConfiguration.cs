using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json;
using System;

namespace ConfigurationManager
{
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
        RabbitMQ,
        ThirdParty
    }
}
