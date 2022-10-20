using CachingProvider.LayeredCache;
using Grpc.Core;
using phoenix;
using System.Threading.Tasks;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        public override Task<IsSegmentUsedResponse> IsSegmentUsed(IsSegmentUsedRequest request, ServerCallContext context)
        {
            var isSegmentUsed = _segmentService.IsSegmentUsed(request);
            return Task.FromResult(new IsSegmentUsedResponse() { IsUsed = isSegmentUsed });
        }
    }
}