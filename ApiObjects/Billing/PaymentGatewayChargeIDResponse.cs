using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class PaymentGatewayChargeIDResponse
    {
        public ApiObjects.Response.Status ResponseStatus { get; set; }
        public string ChargeID { get; set; }

        public PaymentGatewayChargeIDResponse()
        {

        }

        public PaymentGatewayChargeIDResponse(PaymentGatewayChargeIDResponse paymentGWChargeIDResponse)
        {
            this.ChargeID = paymentGWChargeIDResponse.ChargeID;
            this.ResponseStatus = paymentGWChargeIDResponse.ResponseStatus;
        }
    }
}
