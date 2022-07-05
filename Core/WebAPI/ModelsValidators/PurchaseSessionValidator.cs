using System;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class PurchaseSessionValidator
    {
        public static void Validate(this KalturaPurchaseSession model)
        {
            if (model.ProductId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPurchaseSession.productId");
            }

            if (model.ProductType == KalturaTransactionType.ppv && (!model.ContentId.HasValue || model.ContentId.Value == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPurchaseSession.contentId");
            }
        }
    }
}