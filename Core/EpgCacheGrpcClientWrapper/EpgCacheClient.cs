using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using OTT.Service.EpgCache;

namespace EpgCacheGrpcClientWrapper
{
    public class EpgCacheClient: IEpgCacheClient
    {
        private readonly Epgcache.EpgcacheClient _client;

        public EpgCacheClient(Epgcache.EpgcacheClient client)
        {
            _client = client;
        }

        public Task<Empty> InvalidateEpgAsync(int groupId, long liveAssetId, long startDate, long endDate)
        {
            return _client.InvalidateEpgAsync(new EpgInvalidationRequest
                {LiveAssetId = liveAssetId, StartDate = startDate, EndDate = endDate}).ResponseAsync;
        }
    }
}