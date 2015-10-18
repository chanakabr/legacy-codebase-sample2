using ApiObjects.Response;

namespace ApiObjects.Billing
{
    public class PaymentGatewayItemResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public PaymentGateway PaymentGateway { get; set; }

        public PaymentGatewayItemResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            PaymentGateway = new PaymentGateway();
        }

        public PaymentGatewayItemResponse(ApiObjects.Response.Status resp, PaymentGateway paymentGateway)
        {
            this.Status = resp;
            this.PaymentGateway = paymentGateway;
        }
    }

}
