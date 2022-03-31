using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IProgramAssetCrudMessageService
    {
        Task PublishCreateEventAsync(long groupId, long epgId);
        Task PublishCreateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds);
        Task PublishUpdateEventAsync(long groupId, long epgId);
        Task PublishUpdateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds);
        Task PublishDeleteEventAsync(long groupId, long epgId);
        Task PublishDeleteEventsAsync(long groupId, IReadOnlyCollection<long> epgIds); 
    }
}