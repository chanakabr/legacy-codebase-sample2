using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EpgCacheGrpcClientWrapper
{
    public interface IEpgCacheClient
    {
        Task<Empty> InvalidateEpgAsync(int groupId, long liveAssetId, long startDate, long endDate);
    }
}