using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Prometheus;

namespace Core.Metrics.Web
{
    public static class PrometheusMetricsExtensions
    {
        public static void AddPrometheus(this IApplicationBuilder app)
        {
            if (MetricsHelper.IsMetricsEnabled())
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapMetrics();
                });   
            }
        }

        /// <summary>
        /// Multiple calls for UseEndpoints might break your configurations (https://github.com/dotnet/aspnetcore/issues/17750)
        /// So please, use this custom extension inside UseEndpoints method if needed.
        /// </summary>
        public static void MapPrometheusMetrics(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            if (MetricsHelper.IsMetricsEnabled())
            {
                endpointRouteBuilder.MapMetrics();   
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="registry"></param>
        /// <typeparam name="T">Once we starting use asynchronous method, we should add generic constraint to verify that <see cref="T"/> is of type <see cref="IMiddleware"/></typeparam>
        public static void UsePrometheusMetricsMiddleware<T>(this IApplicationBuilder app, CollectorRegistry registry = null)
        {
            if (MetricsHelper.IsMetricsEnabled())
            {
                app.UseMiddleware<T>(registry ?? Prometheus.Metrics.DefaultRegistry);
            }
        }
    }
}