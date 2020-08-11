
namespace ApiObjects.Billing
{
    public class SubscriptionCycle
    {
        public UnifiedBillingCycle UnifiedBillingCycle { get; set; }
        public bool HasCycle { get; set; }
        public Duration SubscriptionLifeCycle { get; set; }
        public int PaymentGatewayId { get; set; }
    }
}