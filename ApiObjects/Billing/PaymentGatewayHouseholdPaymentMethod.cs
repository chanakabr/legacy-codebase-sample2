
namespace ApiObjects.Billing
{
    public class PaymentGatewayHouseholdPaymentMethod
    {
        public int Id { get; set; }
        public int PaymentGatewayId { get; set; }
        public long HouseholdId { get; set; }
        public int PaymentMethoId { get; set; }
        public string paymentMethodExternalId { get; set; }
        public string PaymentDetails { get; set; }

        public PaymentGatewayHouseholdPaymentMethod()
        {

        }
    }
}
