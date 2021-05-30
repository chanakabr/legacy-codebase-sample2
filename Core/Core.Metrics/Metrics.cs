using System;
using Core.Metrics.Internals;
using Prometheus;
using Prometheus.DotNetRuntime;

namespace Core.Metrics
{
    public static class Metrics
    {
        public static void CollectDefault()
        {
            var builder = DotNetRuntimeStatsBuilder.Customize();
            if (MetricsBuilderActivator.Instance.Activate(builder))
            {
                builder.StartCollecting();
            }
        }

        public static void CollectDefaultAndStartServer()
        {
            // As of now we could prevent whole metrics server to be started, but later we're going to turn on/off specific runtime metrics.
            if (MetricsHelper.IsMetricsEnabled())
            {
                CollectDefault();

                var port = int.Parse(Environment.GetEnvironmentVariable("METRICS_PORT") ?? "8080");
                var metricsServer = new MetricServer(port);
                metricsServer.Start();
            }
        }
    }
}