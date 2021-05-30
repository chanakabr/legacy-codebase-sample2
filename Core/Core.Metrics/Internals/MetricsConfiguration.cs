using System;

namespace Core.Metrics.Internals
{
    internal class MetricsConfiguration
    {
        private readonly RuntimeMetricsEnvironmentParser _parser;
        internal static MetricsConfiguration Instance => Lazy.Value;

        private static readonly Lazy<MetricsConfiguration> Lazy = new Lazy<MetricsConfiguration>(() => new MetricsConfiguration(new RuntimeMetricsEnvironmentParser())); 
        internal const string ENABLE_PROM_METRICS = "ENABLE_PROM_METRICS";
        internal const string RUNTIME_PROM_METRICS = "RUNTIME_PROM_METRICS";

        public bool IsMetricsEnabled { get; private set; }

        public RuntimeMetric[] Metrics { get; private set; }

        public MetricsConfiguration(RuntimeMetricsEnvironmentParser parser)
        {
            _parser = parser;
            Initialize();
        }

        private void Initialize()
        {
            IsMetricsEnabled = _parser.ParseMetricsEnvironmentVariable(ENABLE_PROM_METRICS);
            Metrics = _parser.ParseRuntimeMetrics(RUNTIME_PROM_METRICS);
        }
    }
}