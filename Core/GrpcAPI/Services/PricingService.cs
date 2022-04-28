using System;
using System.Linq;
using System.Reflection;
using ApiObjects.Billing;
using Core.ConditionalAccess;
using Google.Protobuf.Collections;
using GrpcAPI.Utils;
using Microsoft.Extensions.Logging;
using phoenix;
using Phx.Lib.Log;

namespace GrpcAPI.Services
{
    public interface IPricingService
    {
        GetItemsPricesResponse GetItemsPrices(GetItemsPricesRequest request);
        GetPaymentGatewayProfileResponse GetPaymentGatewayProfile(GetPaymentGatewayProfileRequest request);
    }

    public class PricingService : IPricingService
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());
        public GetItemsPricesResponse GetItemsPrices(GetItemsPricesRequest request)
        {
            try
            {
                BaseConditionalAccess t = null;
                Core.ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, request.GroupId);
                if (t != null)
                {
                    MediaFileItemPricesContainer[] itemsPrices = t.GetItemsPrices(request.MediaFiles.ToArray(),
                        request.UserId.ToString(),
                        request.CouponCode,
                        request.OnlyLowest, request.LanguageCode, request.Udid,
                        request.Ip, request.CurrencyCode,
                        (ApiObjects.BlockEntitlementType) request.BlockEntitlement, request.IsDownloadPlayContext,
                        request.WithMediaFilesInvalidation);

                    return new GetItemsPricesResponse()
                    {
                        MediaFileItemPrice =
                        {
                            GrpcMapping.Mapper.Map<RepeatedField<MediaFileItemPrice>>(itemsPrices)
                        }
                    };
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while calling GetItemsPrices GRPC service {e.Message}");
                return null;
            }
        }
        
        public GetPaymentGatewayProfileResponse GetPaymentGatewayProfile(GetPaymentGatewayProfileRequest request)
        {
            try
            {
                PaymentGatewayResponse response = Core.Billing.Module.GetPaymentGateway(request.GroupId);
                return new GetPaymentGatewayProfileResponse()
                {
                    PaymentGatewayProfileList =
                    {
                        GrpcMapping.Mapper.Map<RepeatedField<PaymentGatewayProfile>>(response.pgw)
                    }
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while calling GetPaymentGatewayProfile GRPC service {e.Message}");
                return null;
            }
        }
    }
}