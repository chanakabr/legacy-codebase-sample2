using Microsoft.Extensions.Diagnostics.HealthChecks;
using RedisManager;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    class RedisHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = RedisClientManager.Instance.HealthCheck();

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Redis is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Redis is unhealthy"));
            }
        }
    }
}
