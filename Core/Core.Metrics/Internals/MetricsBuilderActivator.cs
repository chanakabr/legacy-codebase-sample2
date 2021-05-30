using System;
using System.Collections.Generic;
using Prometheus.DotNetRuntime;

namespace Core.Metrics.Internals
{
    internal class MetricsBuilderActivator
    {
        internal static MetricsBuilderActivator Instance => Lazy.Value;

        private static readonly Lazy<MetricsBuilderActivator> Lazy = new Lazy<MetricsBuilderActivator>(() => new MetricsBuilderActivator(MetricsConfiguration.Instance));
        private readonly MetricsConfiguration _configuration;
        private static readonly IDictionary<string, Action<DotNetRuntimeStatsBuilder.Builder, RuntimeMetric>> RuntimeMetricsSetup =
            new Dictionary<string, Action<DotNetRuntimeStatsBuilder.Builder, RuntimeMetric>>
            {
                {nameof(DotNetMetric.GC).ToLowerInvariant(), (builder, metric) => { builder.WithGcStats(metric.Level); }},
                {nameof(DotNetMetric.ThreadPool).ToLowerInvariant(), (builder, metric) => { builder.WithThreadPoolStats(metric.Level == CaptureLevel.Verbose ? CaptureLevel.Informational : metric.Level); }}
            };

        public MetricsBuilderActivator(MetricsConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Activate .NET runtime metrics due to configuration.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>Return true in case any metric has been activated, otherwise - false.</returns>
        public bool Activate(DotNetRuntimeStatsBuilder.Builder builder)
        {
            var result = false;
            foreach (var metric in _configuration.Metrics)
            {
                if (RuntimeMetricsSetup.TryGetValue(metric.Metric, out var setup))
                {
                    setup(builder, metric);
                    result = true;
                }
            }

            return result;
        }
    }
}