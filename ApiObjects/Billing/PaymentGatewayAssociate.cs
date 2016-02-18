using System.Collections.Generic;

namespace ApiObjects.Billing
{
    public class PaymentGatewaySelectedBy : PaymentGatewayBase
    {
        public eHouseholdPaymentGatewaySelectedBy By { get; set; }

        public List<HouseholdPaymentMethod> PaymentMethods { get; set; }

        public PaymentGatewaySelectedBy()
        {
        }


    }
}
