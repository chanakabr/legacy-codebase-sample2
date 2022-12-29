using System.Threading.Tasks;
using ApiObjects.Segmentation;

namespace ApiLogic.Modules.Services
{
    public interface IHouseholdSegmentCrudMessageService
    {
        Task PublishCreateEventAsync(long groupId, HouseholdSegment householdSegment);
        Task PublishDeleteEventAsync(long groupId, HouseholdSegment householdSegment);

        Task PublishMigrationCreateEventAsync(long groupId, HouseholdSegment householdSegment);
    }
}