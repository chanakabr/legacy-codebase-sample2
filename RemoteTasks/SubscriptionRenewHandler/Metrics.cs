using ApiObjects;
using Counter = OTT.Lib.Metrics.Metrics.Counter;

namespace SubscriptionRenewHandler
{
    public static class Metrics
    {
        private static readonly string domainMetrics = "subscription_renew";
        private static readonly Counter RenewCounter = new Counter($"{domainMetrics}_tasks_total", "", new[] { "type", "result", "groupId" });

        public static void Track(eSubscriptionRenewRequestType? type, bool result, int? groupId)
        {
            RenewCounter.Inc(dynamicLabelValues: new[] 
            {
                type.HasValue ? type.Value.ToString().ToLower() : "unknown",
                result ? "true" : "false",
                groupId.HasValue ? groupId.ToString() : "unknown"
            });
        }
    }
}
