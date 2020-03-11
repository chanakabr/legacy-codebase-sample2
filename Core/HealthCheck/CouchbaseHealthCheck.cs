using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CouchbaseManager;

namespace HealthCheck
{
    class CouchbaseHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            bool isHealthy = couchbaseManager.HealthCheck();

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("CouchBase is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("CouchBase is unhealthy"));
            }
        }
    }
}
