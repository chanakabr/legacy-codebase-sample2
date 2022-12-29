using System.Threading.Tasks;
using ApiObjects.Segmentation;

namespace ApiLogic.Modules.Services
{
    public interface ISegmentationTypeCrudMessageService
    {
        Task PublishCreateEventAsync(int groupId, SegmentationType segType);
        Task PublishUpdateEventAsync(int groupId, SegmentationType segType);
        Task PublishDeleteEventAsync(long groupId, long segTypeId);
        Task PublishMigrationCreateEventAsync(long groupId, SegmentationType segType);

    }
}