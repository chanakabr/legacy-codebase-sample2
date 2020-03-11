using Microsoft.Extensions.Diagnostics.HealthChecks;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    class RabbitHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            RabbitQueue queue = new RabbitQueue();
            bool isHealthy = queue.HealthCheck();

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ is unhealthy"));
            }
        }
    }
}
