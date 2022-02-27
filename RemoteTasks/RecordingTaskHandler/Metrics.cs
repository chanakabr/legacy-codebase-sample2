using ApiObjects;
using Counter = OTT.Lib.Metrics.Metrics.Counter;

namespace RecordingTaskHandler
{
    public static class Metrics
    {
        private static readonly string domainMetrics = "recording";

        private static readonly Counter RecordingsCounter = new Counter($"{domainMetrics}_tasks_total", "", new[] { "type", "result", "groupId" });

        public static void Track(eRecordingTask? type, bool result, int? groupId)
        {
            RecordingsCounter.Inc(dynamicLabelValues: new[] 
            {               
                type.HasValue ? type.Value.ToString().ToLower() : "unknown",
                result ? "true" : "false", 
                groupId.HasValue ? groupId.ToString() : "unknown"
            });
        }
    }
}
