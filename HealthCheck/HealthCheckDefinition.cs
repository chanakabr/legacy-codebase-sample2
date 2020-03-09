using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCheck
{
    public class HealthCheckDefinition
    {
        public HealthCheckType Type;
        public object[] Args;

        public HealthCheckDefinition(HealthCheckType type, object[] args)
        {
            Type = type;
            Args = args;
        }

        public HealthCheckDefinition(ConfigurationManager.HealthCheckDefinition definition)
        {
            Enum.TryParse<HealthCheckType>(definition.Type.ToString(), out this.Type);
            this.Args = definition.Args;
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
