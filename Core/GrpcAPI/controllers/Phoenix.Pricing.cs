using CachingProvider.LayeredCache;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using phoenix;
using System.Threading.Tasks;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        public override Task<GetItemsPricesResponse> GetItemsPrices(GetItemsPricesRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_pricingService.GetItemsPrices(request));
        }

        public override Task<GetPaymentGatewayProfileResponse> GetPaymentGatewayProfile(
            GetPaymentGatewayProfileRequest request, ServerCallContext context)
        { 
            return Task.FromResult(_pricingService.GetPaymentGatewayProfile(request));
        }
        
        public override Task<BoolValue> GetGroupHasSubWithAds(
            GetGroupHasSubWithAdsRequest request, ServerCallContext context)
        {
            var response = _pricingService.GetGroupHasSubWithAds(request);
            return Task.FromResult(new BoolValue {Value = response});
        }
        
        public override Task<BoolValue> IsMediaFileFree(IsMediaFileFreeRequest request, ServerCallContext context)
        {
            var response = _pricingService.IsMediaFileFree(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest)); 
            return Task.FromResult(new BoolValue { Value =  response});
        }
    }
}