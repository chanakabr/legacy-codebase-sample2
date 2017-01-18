
namespace ApiObjects.Billing
{
    public class PaymentGatewayHouseholdPaymentMethod
    {
        public int Id { get; set; }
        public int PaymentGatewayId { get; set; }
        public long HouseholdId { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodExternalId { get; set; }
        public string PaymentDetails { get; set; }
        public bool Selected { get; set; }

        public PaymentGatewayHouseholdPaymentMethod()
        {

        }
    }
}
