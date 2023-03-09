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
        public override Task<GetAssetUserRuleByUserIdResponse> GetAssetUserRuleByUserId(GetAssetUserRuleByUserIdRequest request, ServerCallContext context)
        {
            var assetUserRuleId = _assetUserRuleService.GetAssetUserRuleByUserId(request);
            return Task.FromResult(new GetAssetUserRuleByUserIdResponse() { AssetUserRuleId = assetUserRuleId });
        }

        public override Task<IsAssetUserRuleIdValidResponse> IsAssetUserRuleIdValid(IsAssetUserRuleIdValidRequest request, ServerCallContext context)
        {
            bool isAssetUserRuleIdValid = _assetUserRuleService.IsAssetUserRuleIdValid(request);
            return Task.FromResult(new IsAssetUserRuleIdValidResponse() { IsValid = isAssetUserRuleIdValid });
        }
    }
}