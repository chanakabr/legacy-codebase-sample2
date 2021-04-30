using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace Phoenix.Rest.Middleware.Metrics
{
    internal static class PrometheusMetricsExtensions
    {
        internal static void UseHttpPrometheusMetrics(this IApplicationBuilder app, CollectorRegistry registry = null)
        {
            app.UseMiddleware<PhoenixHttpRequestDurationMiddleware>(registry ?? Prometheus.Metrics.DefaultRegistry);
        }
    }
}