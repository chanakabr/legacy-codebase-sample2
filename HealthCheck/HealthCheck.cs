using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace HealthCheck
{
    public static class HealthCheck
    {
        public static void AddHealthCheckService(this IServiceCollection services)
        {
            var healthCheckBuilder = services.AddHealthChecks();
            healthCheckBuilder = healthCheckBuilder.AddCheck<ElasticSearchHealthCheck>("elastic_search_check");
        }

        public static void UseHealthCheck(this IApplicationBuilder app, string path)
        {
            var options = new HealthCheckOptions();
            //options.ResponseWriter = 
            //options.ResultStatusCodes.Add(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy, 200);
            //options.ResultStatusCodes.Add(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy, 503);
            //options.ResultStatusCodes.Add(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded, 503);
            
            app.UseHealthChecks(path, options);
        }

        //public static HttpContext GetHealthCheckResponse(HealthReport report)
        //{
        //    HttpContext.
        //}
    }
}
