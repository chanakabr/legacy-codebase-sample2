using System;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class ExternalReceiptValidator
    {
        public static void Validate(this KalturaExternalReceipt model)
        {
            // validate purchase token
            if (string.IsNullOrEmpty(model.ReceiptId))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaExternalReceipt.receiptId");

            // validate payment gateway id
            if (string.IsNullOrEmpty(model.PaymentGatewayName))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaExternalReceipt.paymentGatewayName");
        }
    }
}