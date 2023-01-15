using CachingProvider.LayeredCache;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using phoenix;
using System.Threading.Tasks;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        public override Task<BoolValue> IsPermittedPermission(IsPermittedPermissionRequest request,
            ServerCallContext context)
        {
            var response = _householdService.IsPermittedPermission(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new BoolValue {Value = response});
        }

        public override Task<GetSuspensionStatusResponse> GetSuspensionStatus(GetSuspensionStatusRequest request,
            ServerCallContext context)
        {
            var response = _householdService.GetSuspensionStatus(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetDomainDataResponse> GetDomainData(GetDomainDataRequest request,
            ServerCallContext context)
        {
            var response = _householdService.GetDomainData(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetMediaConcurrencyRulesByDomainLimitationModuleResponse>
            GetMediaConcurrencyRulesByDomainLimitationModule(
                GetMediaConcurrencyRulesByDomainLimitationModuleRequest request, ServerCallContext context)
        {
            var response = _householdService.GetMediaConcurrencyRulesByDomainLimitationModule(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }
        
        public override Task<Int32Value> IsDevicePlayValid(
            IsDevicePlayValidRequest request, ServerCallContext context)
        {
            var response = _householdService.IsDevicePlayValid(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new Int32Value {Value = response});
        }

        public override Task<BoolValue> AllowActionInSuspendedDomain(
            AllowActionInSuspendedDomainRequest request, ServerCallContext context)
        {
            var response = _householdService.AllowActionInSuspendedDomain(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new BoolValue {Value = response});
        }
        
        public override Task<BoolValue> IsValidDeviceFamily(
            IsValidDeviceFamilyRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(new BoolValue {Value = _householdService.IsValidDeviceFamily(request)});
        }
        
        public override Task<GetUserDataResponse> GetUserData(
            GetUserDataRequest request,
            ServerCallContext context)
        {
            var response = _householdService.GetUserData(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }
    }
}