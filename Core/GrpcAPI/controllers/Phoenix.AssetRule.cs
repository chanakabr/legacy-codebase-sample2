using CachingProvider.LayeredCache;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using phoenix;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        public override Task<GetAssetMediaRuleIdsResponse> GetAssetMediaRuleIds(GetAssetMediaRuleIdsRequest request,
            ServerCallContext context)
        {
            var response = _assetRuleService.GetAssetMediaRuleIds(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetAssetEpgRuleIdsResponse> GetAssetEpgRuleIds(GetAssetEpgRuleIdsRequest request,
            ServerCallContext context)
        {
            var response = _assetRuleService.GetAssetEpgRuleIds(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetMediaConcurrencyRulesResponse> GetMediaConcurrencyRules(
            GetMediaConcurrencyRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.GetMediaConcurrencyRules(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<BoolValue>
            HasAssetRules(
                HasAssetRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.HasAssetRules(request);
            var invalidationKeyFromRequest = new List<string>{LayeredCacheKeys.GetAllAssetRulesGroupInvalidationKey(request.GroupId)};
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new BoolValue{Value = response});
        }

        public override Task<CheckNetworkRulesResponse>
            CheckNetworkRules(
                CheckNetworkRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.CheckNetworkRules(request);
            return Task.FromResult(response);
        }

        public override Task<GetAssetRulesResponse>
            GetAssetRules(
                GetAssetRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.GetAssetRules(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetGroupMediaConcurrencyRulesResponse>
            GetGroupMediaConcurrencyRules(
                GetGroupMediaConcurrencyRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.GetGroupMediaConcurrencyRules(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetMediaConcurrencyByIdResponse>
            GetMediaConcurrencyRule(
                GetMediaConcurrencyByIdRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.GetMediaConcurrencyRule(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }
    }
}