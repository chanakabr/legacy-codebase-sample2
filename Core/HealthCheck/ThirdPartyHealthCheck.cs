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

        private readonly HttpClient httpClient = null;

        public ThirdPartyHealthCheck(IHttpClientFactory factory, string name, string healthCheckUrl)
        {
            httpClient = factory.CreateClient(name);

            this.name = name;
            this.healthCheckUrl = healthCheckUrl;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = false;

            try
            {
                var response = await httpClient.GetAsync(healthCheckUrl);

                isHealthy = response.IsSuccessStatusCode;
            }
            catch
            {
                isHealthy = false;
            }

            if (isHealthy)
            {
                return await Task.FromResult(HealthCheckResult.Healthy($"{name} is healthy"));
            }
            else
            {
                return await Task.FromResult(HealthCheckResult.Unhealthy($"{name} is unhealthy"));
            }
        }
    }
}
