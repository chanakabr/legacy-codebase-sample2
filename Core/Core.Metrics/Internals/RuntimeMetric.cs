using Prometheus.DotNetRuntime;

namespace Core.Metrics.Internals
{
    internal class RuntimeMetric
    {
        public RuntimeMetric(string metric, CaptureLevel level)
        {
            Metric = metric.ToLowerInvariant();
            Level = level;
        }

        public string Metric { get; }

        public CaptureLevel Level { get; }
    }
}