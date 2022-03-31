using System.Threading.Tasks;
using ApiObjects.Segmentation;

namespace ApiLogic.Modules.Services
{
    public interface IUserSegmentCrudMessageService
    {
        Task PublishCreateEventAsync(long groupId, UserSegment userSegment);
        Task PublishDeleteEventAsync(long groupId, UserSegment userSegment);
    }
}