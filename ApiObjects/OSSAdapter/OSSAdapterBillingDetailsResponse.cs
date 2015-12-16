using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class OSSAdapterBillingDetailsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public string ChargeId { get; set; }
        
        public string PaymentGatewayId { get; set; }


        public OSSAdapterBillingDetailsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);            
        }

        public OSSAdapterBillingDetailsResponse(ApiObjects.Response.Status status, string chargeId, string paymentGatewayId)
        {
            this.Status = status;
            this.ChargeId = chargeId;
            this.PaymentGatewayId = paymentGatewayId;
        }
    }
}
