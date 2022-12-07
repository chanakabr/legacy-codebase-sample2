using System.Collections.Generic;
using System.Threading.Tasks;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiObjects.BulkUpload;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IProgramAssetCrudMessageService
    {
        Task PublishCreateEventAsync(long groupId, long epgId, long updaterId);
        Task PublishCreateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds, long updaterId);
        Task PublishUpdateEventAsync(long groupId, long epgId, long updaterId);
        Task PublishUpdateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds, long updaterId);
        Task PublishDeleteEventAsync(long groupId, EpgAsset deletedEpg, long updaterId);
        Task PublishDeleteEventsAsync(long groupId, IEnumerable<EpgProgramBulkUploadObject> deletedEpgs, long updaterId);
    }
}