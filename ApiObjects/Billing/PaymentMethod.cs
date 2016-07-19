using System.Collections.Generic;

namespace ApiObjects.Billing
{
    public class PaymentMethod
    {
        public int ID { get; set; }
        public int PaymentGatewayId { get; set; }
        public string Name { get; set; }
        public bool AllowMultiInstance { get; set; }
    }

    public class PaymentMethodsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<PaymentMethod> PaymentMethods { get; set; }
    }

    public class PaymentMethodResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
    }

    public class PaymentGatwayPaymentMethods
    {
        public PaymentMethod PaymentMethod  { get; set; }
        public List<HouseholdPaymentMethod> HouseHoldPaymentMethods { get; set; }
    }

    public class HouseholdPaymentMethod
    {
        public int ID { get; set; }
        public string Details { get; set; }
        public bool Selected { get; set; }
    }
}
