using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using phoenix;
using System.Threading.Tasks;
using CachingProvider.LayeredCache;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        public override Task<GetGroupSecretAndCountryCodeResponse> GetGroupSecretAndCountryCode(
            GetGroupSecretAndCountryCodeRequest request, ServerCallContext context)
        {
            return Task.FromResult(_groupAndConfigurationService.GetGroupSecretAndCountryCode(request));
        }

        //TODO add invalidation keys for client cache cache
        public override Task<GetCDVRAdapterResponse> GetCDVRAdapter(GetCDVRAdapterRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_groupAndConfigurationService.GetCDVRAdapter(request));
        }
        
        public override Task<BoolValue> IsRegionalization(IsRegionalizationRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(new BoolValue {Value = _groupAndConfigurationService.IsRegionalization(request)});
        }

        public override Task<GetDefaultRegionIdResponse> GetDefaultRegionId(GetDefaultRegionIdRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_groupAndConfigurationService.GetDefaultRegionId(request));
        }

        public override Task<GetNotificationPartnerSettingsResponse> GetNotificationPartnerSettings(
            GetNotificationPartnerSettingsRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_groupAndConfigurationService.GetNotificationPartnerSettings(request));
        }
        
        public override async Task<GetConcurrencyMilliThresholdResponse> GetConcurrencyMilliThreshold(
            GetConcurrencyMilliThresholdRequest request,
            ServerCallContext context)
        {
            var response = _groupAndConfigurationService.GetConcurrencyMilliThreshold(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }
        
        public override async Task<GetGeneralPartnerConfigResponse> GetGeneralPartnerConfig(
            GetGeneralPartnerConfigRequest request,
            ServerCallContext context)
        {
            var response = _groupAndConfigurationService.GetGeneralPartnerConfig(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }
    }
}