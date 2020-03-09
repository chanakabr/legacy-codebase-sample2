using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCheck
{
    public class HealthCheckDefinition
    {
        public HealthCheckDefinition(HealthCheckType type, object[] args)
        {

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
