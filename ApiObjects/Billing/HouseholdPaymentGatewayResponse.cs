using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class HouseholdPaymentGatewayResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public PaymentGatewayBase PaymentGateway { get; set; }
        public eHouseholdPaymentGatewaySelectedBy SelectedBy { get; set; }

        public HouseholdPaymentGatewayResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
        }

        public HouseholdPaymentGatewayResponse(ApiObjects.Response.Status status, PaymentGatewayBase paymentGateway, eHouseholdPaymentGatewaySelectedBy selectedBy)
        {
            this.Status = status;
            this.PaymentGateway = paymentGateway;
            this.SelectedBy = selectedBy;
        }
    }
}
