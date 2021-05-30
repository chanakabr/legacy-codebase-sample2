using Core.Metrics.Internals;

namespace Core.Metrics
{
    public static class MetricsHelper
    {
        public static bool IsMetricsEnabled()
        {
            return MetricsConfiguration.Instance.IsMetricsEnabled;
        }
    }
}