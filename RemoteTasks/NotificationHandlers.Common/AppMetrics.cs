using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Phx.Lib.Log;
using Prometheus;

namespace NotificationHandlers.Common
{
    public class AppMetrics
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Counter Events = Metrics.CreateCounter("handler_events_total", "Number of events", "status");

        private static readonly Histogram EventHandleDuration =
            Metrics.CreateHistogram("handler_event_duration_seconds", "Duration of events handling",
                new HistogramConfiguration {Buckets = new double[]{10, 20, 40, 60, 90, 120, 150, 180, 240, 300, 420}});

        public static void Start()
        {
            var port = int.Parse(Environment.GetEnvironmentVariable("METRICS_PORT") ?? "8080");
            Start(port);
        }

        public static void Start(int port)
        {
            var metricServer = new MetricServer(port);
            metricServer.Start();
            Logger.Info($"Metrics are available at {{hostname}}:{port}/metrics");
        }

        public static ITimer EventDuration() => EventHandleDuration.NewTimer(); // handler_event_duration_seconds_count  = filtered + succeed + failed
        public static void EventFiltered() => Events.WithLabels("filtered").Inc();
        public static void EventSucceed() => Events.WithLabels("succeed").Inc();
        public static void EventFailed() => Events.WithLabels("failed").Inc();
    }

    public class RequestMetric
    {
        private static readonly Histogram Duration = Metrics.CreateHistogram("request_duration_seconds", "Duration of requests", "service", "action");
        private static readonly Counter Requests = Metrics.CreateCounter("requests_total", "Number of requests", "service", "action", "status");

        private readonly Histogram.Child _duration;
        private readonly Counter.Child _succeed;
        private readonly Counter.Child _failed;

        public RequestMetric(string service, string action)
        {
            _duration = Duration.WithLabels(service, action);
            _succeed = Requests.WithLabels(service, action, "succeed");
            _failed = Requests.WithLabels(service, action, "failed");
        }

        public ITimer RequestDuration() => _duration.NewTimer();

        public void RequestSucceed() => _succeed.Inc();

        public void RequestFailed() => _failed.Inc();
    }
}
