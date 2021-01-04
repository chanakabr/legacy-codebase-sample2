using KLogMonitor;
using Prometheus;
using System;
using System.Reflection;

namespace EpgNotificationHandler
{
    internal class AppMetrics
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Counter _events = Metrics.CreateCounter("handler_events_total", "Number of events", "status");
        private static readonly Histogram _eventHandleDuration = Metrics.CreateHistogram("handler_event_duration_seconds", "Duration of events handling");

        private static readonly Counter _iotRequests = Metrics.CreateCounter("iot_requests_total", "Number of requests to AWS IoT", "status");
        private static readonly Histogram _iotRequestDuration = Metrics.CreateHistogram("iot_request_duration_seconds", "Duration of requests to AWS IoT");

        public static void Start()
        {
            var port = int.Parse(Environment.GetEnvironmentVariable("METRICS_PORT") ?? "8080");
            Start(port);
        }

        public static void Start(int port)
        {
            var metricServer = new MetricServer(port);
            metricServer.Start();
            _logger.Info($"Metrics are available at {{hostname}}:{port}/metrics");
        }

        public static ITimer EventDuration() => _eventHandleDuration.NewTimer(); // handler_event_duration_seconds_count  = filtered + succeed + failed
        public static void EventFiltered() => _events.WithLabels("filtered").Inc();
        public static void EventSucceed() => _events.WithLabels("succeed").Inc();
        public static void EventFailed() => _events.WithLabels("failed").Inc();

        public static ITimer IotRequestDuration() => _iotRequestDuration.NewTimer();
        public static void IotRequestSucceed() => _iotRequests.WithLabels("succeed").Inc();
        public static void IotRequestFailed() => _iotRequests.WithLabels("failed").Inc();        
    }
}
