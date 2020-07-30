
namespace ApiObjects.Billing
{
    public class SubscriptionCycle
    {
        // TODO SHIR - SET TO NULL? ENDDATE
        public UnifiedBillingCycle UnifiedBillingCycle { get; set; }
        public bool HasCycle { get; set; }
        public Duration SubscriptionLifeCycle { get; set; }
        public int PaymentGatewayId { get; set; }
    }
}
