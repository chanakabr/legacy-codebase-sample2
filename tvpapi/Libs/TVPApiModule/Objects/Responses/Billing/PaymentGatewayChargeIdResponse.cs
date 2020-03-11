using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses.Billing
{
    public class PaymentGatewayChargeIdResponse
    {
         [JsonProperty(PropertyName = "charge_Id")]
        public string ChargeId;


        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public PaymentGatewayChargeIdResponse(TVPPro.SiteManager.TvinciPlatform.Billing.PaymentGatewayChargeIDResponse paymentGatewayChargeIDResponse)
        {
            if (paymentGatewayChargeIDResponse != null)
            {
                this.Status = new Responses.Status(paymentGatewayChargeIDResponse.ResponseStatus.Code, paymentGatewayChargeIDResponse.ResponseStatus.Message);
                this.ChargeId = paymentGatewayChargeIDResponse.ChargeID;
            }
        }

        public PaymentGatewayChargeIdResponse()
        {
        }
    }
}
