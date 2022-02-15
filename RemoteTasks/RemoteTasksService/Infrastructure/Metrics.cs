using ApiObjects;
using Counter = OTT.Lib.Metrics.Metrics.Counter;

namespace RemoteTasksService.Infrastructure
{
    public static class Metrics
    {
        private static readonly string domainMetrics = "remote";
        private static readonly Counter RequestsMetricCount = new Counter($"{domainMetrics}_tasks_total", "", new[] { "taskname", "taskresult", "status", "groupId" });

        public static void Track(string taskHandlerName, AddTaskResponse response, int groupId)
        {
            RequestsMetricCount.Inc(dynamicLabelValues: new[]
            {
                string.IsNullOrEmpty(taskHandlerName) ? "unknown" : taskHandlerName,
                response.retval ?? "failure",
                response.status,
                groupId.ToString()
            });
        }
    }
}
