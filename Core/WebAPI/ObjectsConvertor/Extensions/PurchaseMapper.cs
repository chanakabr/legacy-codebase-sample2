using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class PurchaseMapper
    {
        public static int getPaymentMethodId(this KalturaPurchase model)
        {
            return model.PaymentMethodId.HasValue ? model.PaymentMethodId.Value : 0;
        }
        
        public static int getPaymentGatewayId(this KalturaPurchase model)
        {
            return model.PaymentGatewayId.HasValue ? model.PaymentGatewayId.Value : 0;
        }
        
        public static string getCoupon(this KalturaPurchase model)
        {
            return model.Coupon == null ? string.Empty : model.Coupon;
        }
    }
}