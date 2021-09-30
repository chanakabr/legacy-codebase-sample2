using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using OTT.Service.EpgCache;

namespace EpgCacheGrpcClientWrapper
{
    // Regenerate service was the main branch when added this nuget(0.1.33-regenerate-service), can be changed in the future when needed
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

        public Task<PartnerConfigurationResponse> GetEpgCacheServiceConfigurations(PartnerConfigurationRequest req)
        {
            return _client.GetPartnerConfigurationAsync(req).ResponseAsync;
        }

        public Task<Empty> UpdateEpgCacheServiceConfigurations(UpdatePartnerConfigurationRequest config, int partnerId)
        {
            return _client.UpdatePartnerConfigurationAsync(config).ResponseAsync;
        }
    }
}