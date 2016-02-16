
namespace ApiObjects.Billing
{
    public class PaymentGatewaySelectedBy : PaymentGatewayBase
    {
        public eHouseholdPaymentGatewaySelectedBy By { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentGatewaySelectedBy()
        {
        }


    }
}
