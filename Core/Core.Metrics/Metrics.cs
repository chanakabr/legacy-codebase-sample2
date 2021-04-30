using System;
using Prometheus;
using Prometheus.DotNetRuntime;

namespace Core.Metrics
{
    public static class Metrics
    {
        public static void CollectDefault()
        {
            DotNetRuntimeStatsBuilder.Customize()
                .WithThreadPoolSchedulingStats()
                .WithContentionStats()
                .WithGcStats()
                .WithThreadPoolStats()
                .StartCollecting();
        }

        public static void CollectDefaultAndStartServer()
        {
            CollectDefault();
            
            var port = int.Parse(Environment.GetEnvironmentVariable("METRICS_PORT") ?? "8080");
            var metricsServer = new MetricServer(port);
            metricsServer.Start();
        }
    }
}