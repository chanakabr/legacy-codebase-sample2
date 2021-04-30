using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Prometheus;

namespace Core.Metrics.Web
{
    public static class PrometheusMetricsExtensions
    {
        public static void AddPrometheus(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
            });
        }

        /// <summary>
        /// Multiple calls for UseEndpoints might break your configurations (https://github.com/dotnet/aspnetcore/issues/17750)
        /// So please, use this custom extension inside UseEndpoints method if needed.
        /// </summary>
        public static void MapPrometheusMetrics(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapMetrics();
        }
    }
}