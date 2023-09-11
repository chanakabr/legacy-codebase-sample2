using CachingProvider.LayeredCache;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using phoenix;
using System.Threading.Tasks;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        public override Task<BoolValue> IsServiceAllowed(IsServiceAllowedRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BoolValue()
            {
                Value = _entitlementService.IsServiceAllowed(request),
            });
        }

        public override async Task<GetDomainAdsControlResponse> GetDomainAdsControl(GetDomainAdsControlRequest request,
            ServerCallContext context)
        {
            var response = _entitlementService.GetDomainAdsControl(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override Task<GetPPVModuleDataResponse> GetPPVModuleData(GetPPVModuleDataRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_entitlementService.GetPPVModuleData(request));
        }

        public override Task<Empty> HandlePlayUses(
            HandlePlayUsesRequest request, ServerCallContext context)
        {
            return Task.FromResult(_entitlementService.HandlePlayUses(request));
        }

        public override async Task<BoolValue> CheckProgramAssetGroupExistence(CheckProgramAssetGroupExistenceRequest request,
            ServerCallContext context)
        {
            var response = _entitlementService.CheckProgramAssetGroupExistence(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return new BoolValue{Value = response};        
        }
        
        public override Task<GetEntitledPagoWindowResponse> GetEntitledPagoWindow(GetEntitledPagoWindowRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_entitlementService.GetEntitledPagoWindow(request));
        }
    }
}