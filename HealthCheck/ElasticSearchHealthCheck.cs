using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    class ElasticSearchHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            ElasticSearch.Common.ElasticSearchApi api = new ElasticSearch.Common.ElasticSearchApi();
            bool healthCheckResultHealthy = api.HealthCheck();

            if (healthCheckResultHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("ElasticSearch is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("ElasticSearch is unhealthy"));
            }
        }
    }
}
