using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace HealthCheck
{
    class ThirdPartyHealthCheck : IHealthCheck
    {
        private string name;
        private string healthCheckUrl;

        private readonly HttpClient httpClient = new HttpClient();

        public ThirdPartyHealthCheck(string name, string healthCheckUrl)
        {
            this.name = name;
            this.healthCheckUrl = healthCheckUrl;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = false;
            var response = Task.Run(() => httpClient.GetAsync(healthCheckUrl)).ConfigureAwait(false).GetAwaiter().GetResult();
            int statusCode = (int)response.StatusCode;
            isHealthy = statusCode >= 200 && statusCode < 300;

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy($"{name} is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy($"{name} is unhealthy"));
            }
        }
    }
}
