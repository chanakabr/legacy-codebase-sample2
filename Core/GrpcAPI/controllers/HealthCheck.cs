using System.Threading.Tasks;
using Grpc.Core;
using grpc.health.v1;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GrpcAPI.controllers
{
    public class HealthCheckController : Health.HealthBase
    {
        private readonly HealthCheckService _healthCheckService;

        public HealthCheckController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        public override async Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            var result = await _healthCheckService.CheckHealthAsync(context.CancellationToken);

            var status = result.Status == HealthStatus.Healthy ? HealthCheckResponse.Types.ServingStatus.Serving : HealthCheckResponse.Types.ServingStatus.NotServing;

            return new HealthCheckResponse
            {
                Status = status
            };
        }
    }
}