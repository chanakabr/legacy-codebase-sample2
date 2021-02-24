using Microsoft.Extensions.Diagnostics.HealthChecks;
using RedisManager;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck
{
    class PersistentRedisHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool isHealthy = RedisClientManager.PersistenceInstance.HealthCheck();

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Persistent Redis is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Persistent Redis is unhealthy"));
            }
        }
    }
}
