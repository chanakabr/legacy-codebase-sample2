using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    class ThirdPartyHealthCheck : IHealthCheck
    {
        private string name;
        private string healthCheckUrl;

        public ThirdPartyHealthCheck(string name, string healthCheckUrl)
        {
            this.name = name;
            this.healthCheckUrl = healthCheckUrl;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy($"{name} is healthy"));
        }
    }
}
