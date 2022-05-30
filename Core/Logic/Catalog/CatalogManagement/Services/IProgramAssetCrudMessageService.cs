using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IProgramAssetCrudMessageService
    {
        Task PublishCreateEventAsync(long groupId, long epgId, long updaterId);
        Task PublishCreateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds, long updaterId);
        Task PublishUpdateEventAsync(long groupId, long epgId, long updaterId);
        Task PublishUpdateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds, long updaterId);
        Task PublishDeleteEventAsync(long groupId, long epgId, long updaterId);
        Task PublishDeleteEventsAsync(long groupId, IReadOnlyCollection<long> epgIds, long updaterId);
    }
}