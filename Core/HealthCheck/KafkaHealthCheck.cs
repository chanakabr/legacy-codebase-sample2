using EventBus.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    class KafkaHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var publisher = (KafkaPublisher)KafkaPublisher.GetFromTcmConfiguration();
            bool isHealthy = publisher.HealthCheck();

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Kafka is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Kafka is unhealthy"));
            }
        }
    }
}
