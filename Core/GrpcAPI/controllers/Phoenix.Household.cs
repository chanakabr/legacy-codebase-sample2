using CachingProvider.LayeredCache;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using phoenix;
using System.Threading.Tasks;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        public override async Task<BoolValue> IsPermittedPermission(IsPermittedPermissionRequest request,
            ServerCallContext context)
        {
            var response = _householdService.IsPermittedPermission(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return new BoolValue {Value = response};
        }

        public override async Task<GetSuspensionStatusResponse> GetSuspensionStatus(GetSuspensionStatusRequest request,
            ServerCallContext context)
        {
            var response = _householdService.GetSuspensionStatus(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override async Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request, ServerCallContext context)
        {
            var response = _householdService.ValidateUser(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override async Task<GetDomainDataResponse> GetDomainData(GetDomainDataRequest request,
            ServerCallContext context)
        {
            var response = _householdService.GetDomainData(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override async Task<GetMediaConcurrencyRulesByDomainLimitationModuleResponse>
            GetMediaConcurrencyRulesByDomainLimitationModule(
                GetMediaConcurrencyRulesByDomainLimitationModuleRequest request, ServerCallContext context)
        {
            var response = _householdService.GetMediaConcurrencyRulesByDomainLimitationModule(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }
        
        public override async Task<Int32Value> IsDevicePlayValid(
            IsDevicePlayValidRequest request, ServerCallContext context)
        {
            var response = _householdService.IsDevicePlayValid(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return new Int32Value {Value = response};
        }

        public override async Task<BoolValue> AllowActionInSuspendedDomain(
            AllowActionInSuspendedDomainRequest request, ServerCallContext context)
        {
            var response = _householdService.AllowActionInSuspendedDomain(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return new BoolValue {Value = response};
        }
        
        public override Task<BoolValue> IsValidDeviceFamily(
            IsValidDeviceFamilyRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(new BoolValue {Value = _householdService.IsValidDeviceFamily(request)});
        }
    }
}