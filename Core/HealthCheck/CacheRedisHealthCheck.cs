using Microsoft.Extensions.Diagnostics.HealthChecks;
using RedisManager;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    class CacheRedisHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = RedisClientManager.CacheInstance.HealthCheck();

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Cache Redis is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Cache Redis is unhealthy"));
            }
        }
    }
}
