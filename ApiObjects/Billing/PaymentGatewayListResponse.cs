using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects.Billing
{
    public class PaymentGatewayListResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<PaymentGatewaySelectedBy> PaymentGateways { get; set; }

        public PaymentGatewayListResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            PaymentGateways = new List<PaymentGatewaySelectedBy>();
        }

        public PaymentGatewayListResponse(ApiObjects.Response.Status resp, List<PaymentGatewaySelectedBy> paymentGateways)
        {
            this.Status = resp;
            this.PaymentGateways = paymentGateways;
        }
    }
}
